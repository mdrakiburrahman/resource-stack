using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Configuration;

using Microsoft.AzureArcData.Sample.Jobs.Jobs;
using Microsoft.AzureArcData.Sample.Jobs.JobMetadata;
using Microsoft.AzureArcData.Sample.Common.EventSource;
using Newtonsoft.Json;
using Microsoft.AzureArcData.Sample.Common.Constants;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage;

JobManagementClient jobManagementClient = new JobManagementClient(
    documentServiceEndpoint: new Uri(
        ConfigurationManager.AppSettings["documentServiceEndpoint"] ?? "https://localhost:8081"
    ),
    documentAuthorizationKey: ConfigurationManager.AppSettings["documentAuthorizationKey"]
        ?? "KeyMissing",
    executionAffinity: "global",
    eventSource: new BJSEventSource(),
    encryptionUtility: null
);

// 1. Job which is always succeeding and is repeated 5 times
//
var newJob = JobBuilder
    .Create(JobConstants.jobPartitionName, Guid.NewGuid().ToString())
    .WithCallback(typeof(AlwaysSucceedJob))
    .WithMetadata(
        JsonConvert.SerializeObject(new AlwaysSucceedJobMetadata { CallerName = "AzureArcData" })
    )
    .WithRepeatStrategy(5, TimeSpan.FromSeconds(10))
    .WithoutRetryStrategy();

await jobManagementClient.CreateOrUpdateJob(newJob).ConfigureAwait(false);

// 2. Job which is sometimes failing is repeated 5 times and retries failed runs for 3 times
//
newJob = JobBuilder
    .Create(JobConstants.jobPartitionName, Guid.NewGuid().ToString())
    .WithCallback(typeof(SometimesFailsJob))
    .WithMetadata(
        JsonConvert.SerializeObject(
            new SometimesFailsJobMetadata { CallerName = "AzureArcData", ChanceOfFailure = .25 }
        )
    )
    .WithRepeatStrategy(5, TimeSpan.FromSeconds(5))
    .WithRetryStrategy(3, TimeSpan.FromSeconds(5));

await jobManagementClient.CreateOrUpdateJob(newJob).ConfigureAwait(false);

// 3. Job which is always succeeding, is never repeated and never retried
//
newJob = JobBuilder
    .Create(JobConstants.jobPartitionName, Guid.NewGuid().ToString())
    .WithCallback(typeof(AlwaysSucceedJob))
    .WithMetadata(
        JsonConvert.SerializeObject(new AlwaysSucceedJobMetadata { CallerName = "AzureArcData" })
    )
    .WithoutRepeatStrategy()
    .WithoutRetryStrategy();

await jobManagementClient.CreateOrUpdateJob(newJob).ConfigureAwait(false);

for (int i = 0; i < 3; i++)
{
    // 4. Job which is using checkpointing
    //
    newJob = JobBuilder
        .Create(JobConstants.jobPartitionName, Guid.NewGuid().ToString())
        .WithCallback(typeof(CheckpointingJob))
        .WithMetadata(
            JsonConvert.SerializeObject(
                new CheckpointingJobMetadata
                {
                    CallerName = "AzureArcData",
                    CurrentStep = 0,
                    MaxSteps = Random.Shared.Next(5, 20)
                }
            )
        )
        .WithoutRepeatStrategy()
        .WithoutRetryStrategy();

    await jobManagementClient.CreateOrUpdateJob(newJob).ConfigureAwait(false);
}

// 5. Sequencer Job:
//
//                              +---------------------+
//                              | SometimesFailsJob   |
//                              +---------------------+
//                                        ^
//  +------------------+                  |
//  | AlwaysSucceedJob |-------------------
//  +------------------+                  |
//                                        v
//                              +-----------------------+
//                              | CheckpointingJob      |
//                              +-----------------------+
//
//
var sequencerBuilder = SequencerBuilder
    .Create(
        JobConstants.jobPartitionName,
        StorageUtility.EscapeStorageKey(Guid.NewGuid().ToString())
    )
    .WithAction(
        "AlwaysSucceedJob",
        typeof(AlwaysSucceedJob).FullName,
        JsonConvert.SerializeObject(new AlwaysSucceedJobMetadata { CallerName = "AzureArcData" })
    )
    .WithAction(
        "SometimesFailsJob",
        typeof(SometimesFailsJob).FullName,
        JsonConvert.SerializeObject(
            new SometimesFailsJobMetadata { CallerName = "AzureArcData", ChanceOfFailure = 0 }
        )
    )
    .WithAction(
        "CheckpointingJob",
        typeof(CheckpointingJob).FullName,
        JsonConvert.SerializeObject(
            new CheckpointingJobMetadata
            {
                CallerName = "AzureArcData",
                CurrentStep = 0,
                MaxSteps = -1
            }
        )
    )
    .WithDependency("AlwaysSucceedJob", "SometimesFailsJob")
    .WithDependency("AlwaysSucceedJob", "CheckpointingJob")
    .WithFlags(SequencerFlags.DeleteSequencerIfCompleted);

await jobManagementClient
    .CreateSequencer(SequencerType.Distributed, sequencerBuilder)
    .ConfigureAwait(false);

// Print state
//
while (true)
{
    Console.WriteLine("");
    Console.WriteLine(
        $"----------------------------------------------- {DateTime.UtcNow} | STATUS CHECK ------------------------------------------------"
    );
    Console.WriteLine("");
    var jobs = await jobManagementClient.GetJobs(JobConstants.jobPartitionName);
    jobs.ToList()
        .ForEach(job =>
        {
            Console.WriteLine(
                $"JobID: {job.JobId} | CallBack: {job.Callback} | MetaData: {job.Metadata} | Status: {job.State}\n"
                    + $"\tLastExecutionTime: {job.LastExecutionTime} | LastExecutionStatus: {job.LastExecutionStatus} | NextExecutionTime: {job.NextExecutionTime}\n"
                    + $"\tRun: {job.CurrentRepeatCount}/{job.RepeatCount} | Interval: {job.RepeatInterval / 1000}ms"
            );
            if (job.State == JobState.Completed || job.LastExecutionStatus == JobExecutionStatus.Succeeded)
            {
                Console.WriteLine($"\nDeleting job {job.JobId}, as it's marked with State {job.State} and Last Execution Status: {job.LastExecutionStatus}");
                jobManagementClient.DeleteJob(JobConstants.jobPartitionName, job.JobId);
            }
        });

    Console.WriteLine("");
    Console.WriteLine(
        "----------------------------------------------------------------------------------------------------------------------------------------"
    );
    Console.WriteLine("");
    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
}
