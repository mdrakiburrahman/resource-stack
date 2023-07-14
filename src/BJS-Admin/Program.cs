using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Configuration;

using Microsoft.AzureArcData.Sample.Jobs.Jobs;
using Microsoft.AzureArcData.Sample.Jobs.JobMetadata;
using Microsoft.AzureArcData.Sample.Common.EventSource;
using Newtonsoft.Json;
using Microsoft.AzureArcData.Sample.Common.Constants;

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

// Job which is always succeeding and is repeated 5 times
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

// Job which is sometimes failing is repeated 5 times and retries failed runs for 3 times
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

// Job which is always succeeding, is never repeated and never retried
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
    // Job which is using checkpointing
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
                $"JobID: {job.JobId} | CallBack: {job.Callback} | MetaData: {job.Metadata} | Status {job.State}\n"
                    + $"\tLastExecutionTime: {job.LastExecutionTime} | LastExecutionStatus: {job.LastExecutionStatus} | NextExecutionTime: {job.NextExecutionTime}\n"
                    + $"\tRun: {job.CurrentRepeatCount}/{job.RepeatCount} | Interval: {job.RepeatInterval / 1000}ms"
            );
            if (job.State == JobState.Completed)
            {
                Console.WriteLine($"\nDeleting job {job.JobId}, as it's marked complete");
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
