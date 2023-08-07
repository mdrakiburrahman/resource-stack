using System.Configuration;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.AzureArcData.Sample.Jobs.Jobs;
using Microsoft.AzureArcData.Sample.Jobs.JobMetadata;
using Microsoft.AzureArcData.Sample.Common.EventSource;
using Microsoft.AzureArcData.Sample.Common.Constants;
using Microsoft.AzureArcData.Sample.Common.Settings;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs.Configuration;

// Sequencer GUIDs
//
string distributedSequencerDynamicId = StorageUtility.EscapeStorageKey(Guid.NewGuid().ToString());
string linearSequencerStaticId = StorageUtility.EscapeStorageKey(
    "a5082b19-8a6e-4bc5-8fdd-8ef39dfebc39"
);

// Initiate the custom job options
//
JobOptions jobOptions = new JobOptions
{
    DefaultSettings = new JobSettings
    {
        JobTimeout = TimeSpan.Parse("00:07:31"),
        SequencerTimeout = TimeSpan.Parse("00:37:59"),
        Retention = TimeSpan.Parse("01:19:19")
    }
};
CustomJobsConfiguration customJobsConfiguration = new(jobOptions);

// Get backend env-var
//
Backend backend;
if (!Enum.TryParse(Environment.GetEnvironmentVariable(JobConstants.backendEnvVarName), out backend))
{
    throw new Exception(
        $"Please set the {JobConstants.backendEnvVarName} environment variable to one of the following values: {string.Join(", ", Enum.GetNames(typeof(Backend)))}"
    );
}

// Initiate backend based on the environment variable
//
JobManagementClient jobManagementClient;
switch (backend)
{
    case Backend.cosmosdb:

        jobManagementClient = new JobManagementClient(
            documentServiceEndpoint: new Uri(
                ConfigurationManager.AppSettings["documentServiceEndpoint"]
                    ?? "https://localhost:8081"
            ),
            documentAuthorizationKey: ConfigurationManager.AppSettings["documentAuthorizationKey"]
                ?? "KeyMissing",
            executionAffinity: "global",
            eventSource: new BJSEventSource(),
            encryptionUtility: null,
            jobsConfigurationProvider: customJobsConfiguration
        );

        break;

    case Backend.sqlserver:

        jobManagementClient = new SqlJobManagementClient(
            databaseConnectionString: ConfigurationManager.AppSettings["sqlServerConnectionString"],
            jobDefinitionsTableName: JobConstants.jobTableName,
            queueNamePrefix: JobConstants.queueTablePrefix,
            executionAffinity: "global",
            eventSource: new BJSEventSource(),
            encryptionUtility: null,
            jobsConfigurationProvider: customJobsConfiguration
        );
        break;

    default:
        throw new Exception($"This demo doesn't have support for {backend} just yet!");
}

JobBuilder newJob;


// 1. Job which is always succeeding and is repeated 5 times
//
newJob = JobBuilder
    .Create(JobConstants.GetJobPartition(), Guid.NewGuid().ToString())
    .WithCallback(typeof(AlwaysSucceedJob))
    .WithMetadata(
        JsonConvert.SerializeObject(new AlwaysSucceedJobMetadata { CallerName = "AzureArcData" })
    )
    //
    // Times out Job
    //
    .WithTimeout(TimeSpan.FromSeconds(5))
    .WithRepeatStrategy(5, TimeSpan.FromSeconds(5))
    .WithRetryStrategy(3, TimeSpan.FromSeconds(5));

await jobManagementClient.CreateOrUpdateJob(newJob).ConfigureAwait(false);

// 2. Job which is sometimes failing is repeated 5 times and retries failed runs for 3 times
//
newJob = JobBuilder
    .Create(JobConstants.GetJobPartition(), Guid.NewGuid().ToString())
    .WithCallback(typeof(SometimesFailsJob))
    .WithMetadata(
        JsonConvert.SerializeObject(
            new SometimesFailsJobMetadata { CallerName = "AzureArcData", ChanceOfFailure = .25 }
        )
    )
    .WithRepeatStrategy(5, TimeSpan.FromSeconds(5))
    .WithRetryStrategy(3, TimeSpan.FromSeconds(5));

await jobManagementClient.CreateOrUpdateJob(newJob).ConfigureAwait(false);

// 3. Job which is always succeeding, is repeated every day
//
var currentTime = DateTime.UtcNow;
var currentMinute = currentTime.Minute;
var currentHour = currentTime.Hour;

JobRecurrenceSchedule schedule = new JobRecurrenceSchedule()
{
    // Trigger on:
    Minutes = new int[] { currentMinute + 1 }, // +1 minute from the current minute
    Hours = new int[] { currentHour }, // On the current hour
    WeekDays = new DayOfWeek[] { DayOfWeek.Wednesday, DayOfWeek.Thursday } // If it is Wednesday and Thursday
};

newJob = JobBuilder
    .Create(JobConstants.GetJobPartition(), Guid.NewGuid().ToString())
    .WithCallback(typeof(AlwaysSucceedJob))
    .WithMetadata(
        JsonConvert.SerializeObject(new AlwaysSucceedJobMetadata { CallerName = "AzureArcData" })
    )
    .WithRepeatStrategy(
        count: int.MaxValue, // Repeat forever
        interval: 1, // Repeat every 1....
        unit: JobRecurrenceUnit.Day, // ... day
        schedule: schedule // against the schedule defined above
    )
    .WithoutRetryStrategy()
    .WithTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); // On EST

await jobManagementClient.CreateOrUpdateJob(newJob).ConfigureAwait(false);

for (int i = 0; i < 3; i++)
{
    // 4. Job which is using checkpointing
    //
    newJob = JobBuilder
        .Create(JobConstants.GetJobPartition(), Guid.NewGuid().ToString())
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

// 5. Distributed Sequencer Job:
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
// Distributed Sequencers run SometimesFailsJob and CheckpointJob in parallel.
//
// Distributed Sequencers CANNOT be declared using the same SequencerId as they
// fan out and spin up child jobs, which are hard to cancel.
//
var distributedSequencerBuilder = SequencerBuilder
    .Create(JobConstants.GetJobPartition(), distributedSequencerDynamicId)
    //
    // Sequencer E2E timeout
    //
    .WithTimeout(TimeSpan.FromMinutes(10))
    .WithAction(
        "AlwaysSucceedJob",
        typeof(AlwaysSucceedJob).FullName,
        JsonConvert.SerializeObject(new AlwaysSucceedJobMetadata { CallerName = "AzureArcData" }),
        //
        // Configure Action Level settings
        //
        action =>
        {
            //
            // Same story as Linear below, basically, in a Sequencer, the
            // highest withAction's timeout is the one that wins. In this case,
            // AlwaysSucceedJob gets a timeout of 999 seconds.
            //
            action.WithTimeout(TimeSpan.FromSeconds(307));
        }
    )
    .WithAction(
        "SometimesFailsJob",
        typeof(SometimesFailsJob).FullName,
        JsonConvert.SerializeObject(
            //
            // Fail on purpose, on the Nth retry - stops execution of the whole Sequencer
            //
            new SometimesFailsJobMetadata { CallerName = "AzureArcData", ChanceOfFailure = 1 }
        ),
        action =>
        {
            //
            // This works on the Distributed Sequencer, we retry 2 more times (3,
            // including the original attempt)
            //
            action.WithRetryStrategy(
                count: 2,
                interval: TimeSpan.FromSeconds(1), // Starting interval
                mode: JobRecurrenceMode.Linear, // Linear backoff
                minInterval: TimeSpan.FromSeconds(1), // Lower ceiling
                maxInterval: TimeSpan.FromSeconds(5) // Upper ceiling
            );
            //
            // All Actions (Jobs) gets this timeout, which is the highest
            // timeout.
            //
            action.WithTimeout(TimeSpan.FromSeconds(999));
        }
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
    .WithRetention(TimeSpan.FromSeconds(999));

await jobManagementClient
    .CreateSequencer(SequencerType.Distributed, distributedSequencerBuilder)
    .ConfigureAwait(false);

// 6. Linear Sequencer Job:
//
//      Exact same as Distributed (including the dependency declaration),
//      except, in linear, only a single action is executed at a time - which
//      allows us to overwrite and cancel - even when it is running.
//
//      I.e. SometimesFailsJob and CheckpointJob runs in Sequencer, respecting
//      their dependency with AlwaysSucceedJob.
//
var linearSequencerBuilder = SequencerBuilder
    .Create(
        JobConstants.GetJobPartition(),
        //
        // Hard-code GUID on purpose, to show we can overwrite
        //
        linearSequencerStaticId
    )
    .WithAction(
        "AlwaysSucceedJob",
        typeof(AlwaysSucceedJob).FullName,
        JsonConvert.SerializeObject(new AlwaysSucceedJobMetadata { CallerName = "AzureArcData" }),
        //
        // Configure Action Level settings
        //
        action =>
        {
            //
            // Overrides default timeout to 00:4:25 seconds, the single Linear
            // Job takes on this timeout value. But...this won't be the source
            // of truth, because the next Action has a higher timeout, so that
            // one is used!
            //
            action.WithTimeout(TimeSpan.FromSeconds(265));
        }
    )
    .WithAction(
        "SometimesFailsJob",
        typeof(SometimesFailsJob).FullName,
        JsonConvert.SerializeObject(
            //
            // Fail on purpose, on the Nth retry - stops execution of the whole Sequencer
            //
            new SometimesFailsJobMetadata { CallerName = "AzureArcData", ChanceOfFailure = 1 }
        ),
        action =>
        {
            //
            // This works on the Linear Sequencer, we retry 3 more times (4,
            // including the original attempt)
            //
            action.WithRetryStrategy(
                count: 3,
                interval: TimeSpan.FromSeconds(2), // Starting interval
                mode: JobRecurrenceMode.Linear, // Linear backoff
                minInterval: TimeSpan.FromSeconds(1), // Lower ceiling
                maxInterval: TimeSpan.FromSeconds(10) // Upper ceiling
            );
            //
            // This is the source of truth for the timeout of the whole
            // Sequencer, including the AlwaysSucceedJob Action.
            //
            action.WithTimeout(TimeSpan.FromSeconds(361));
        }
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
    //
    // Start right now
    //
    .WithStartTime(DateTime.UtcNow.AddSeconds(0))
    //
    // Sequencer E2E timeout
    //
    .WithTimeout(TimeSpan.FromMinutes(15))
    //
    // Retention period
    //
    .WithRetention(TimeSpan.FromMinutes(69));

// Loop to prove that we can overwrite over and over again
//
for (int i = 0; i < 5; i++)
{
    await jobManagementClient
        .CreateSequencer(SequencerType.Linear, linearSequencerBuilder)
        .ConfigureAwait(false);
}

// 7. Sequencer Job - where one Action can retrieve another Action's Metadata at runtime:
//
var defaultMetadataForBothJobs = new SharedJobMetadata { SecretString = string.Empty, SecretValue = 0 };

var chainedSequencerBuilder = SequencerBuilder
    .Create(
        JobConstants.GetJobPartition(),
        StorageUtility.EscapeStorageKey(Guid.NewGuid().ToString())
    )
    .WithAction(
        "MetadataHandoffStartingJob",
        typeof(MetadataHandoffStartingJob).FullName,
        JsonConvert.SerializeObject(defaultMetadataForBothJobs)
    )
    .WithAction(
        "MetadataHandoffEndingJob",
        typeof(MetadataHandoffEndingJob).FullName,
        JsonConvert.SerializeObject(defaultMetadataForBothJobs)
    )
    .WithDependency("MetadataHandoffStartingJob", "MetadataHandoffEndingJob")
    //
    // Start right now
    //
    .WithStartTime(DateTime.UtcNow.AddSeconds(0))
    //
    // Sequencer E2E timeout
    //
    .WithTimeout(TimeSpan.FromMinutes(15))
    //
    // Retention period
    //
    .WithRetention(TimeSpan.FromMinutes(100));

await jobManagementClient
    .CreateSequencer(SequencerType.Distributed, chainedSequencerBuilder)
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

    var jobs = await jobManagementClient.GetJobs(JobConstants.GetJobPartition());
    jobs.ToList()
        .ForEach(job =>
        {
            bool delete = true;

            // Background Job or Sequencer - could be either
            //
            Console.WriteLine(
                $"JobID: {job.JobId} | CallBack: {job.Callback} | Status: {job.State}\n"
                    + $"\tLastExecutionTime: {job.LastExecutionTime} | LastExecutionStatus: {job.LastExecutionStatus} | NextExecutionTime: {job.NextExecutionTime}\n"
                    + $"\tRun: {job.CurrentRepeatCount}/{job.RepeatCount} | Interval: {job.RepeatInterval / 1000}ms | Timeout: {job.Timeout} | Retention: {job.Retention}"
            );

            // Check if Sequencer
            //
            Task<SequencerAction[]> sequencerActionTasks;
            if (job.SequencerType != SequencerType.NotSpecified)
            {
                // Don't delete Sequencer status
                //
                delete = false;

                if (job.SequencerType == SequencerType.Linear)
                {
                    sequencerActionTasks = jobManagementClient.GetSequencerActions(
                        sequencerType: job.SequencerType,
                        sequencerPartition: JobConstants.GetJobPartition(),
                        sequencerId: linearSequencerStaticId
                    );
                }
                else
                {
                    sequencerActionTasks = jobManagementClient.GetSequencerActions(
                        sequencerType: job.SequencerType,
                        sequencerPartition: JobConstants.GetJobPartition(),
                        sequencerId: distributedSequencerDynamicId
                    );
                }

                // Retrieve all Sequencer Actions synchronously
                //
                SequencerAction[]? sequencerActions = sequencerActionTasks.Result;

                // Print Sequencer Action State
                //
                foreach (var action in sequencerActions)
                {
                    Console.WriteLine("");
                    Console.WriteLine($"ActionId: {action.ActionId}");
                    Console.WriteLine($"Result: {action.Result}");
                    Console.WriteLine($"State: {action.State}");
                    Console.WriteLine("");

                    if (
                        action.Result == SequencerActionResult.Failed
                        || action.Result == SequencerActionResult.TimedOut
                        || action.Result == SequencerActionResult.Skipped
                    )
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"ActionId: {action.ActionId} failed");
                        Console.ResetColor();
                    }
                }
            }

            if (
                (
                    job.State == JobState.Completed
                    || job.LastExecutionStatus == JobExecutionStatus.Succeeded
                ) && delete
            )
            {
                Console.WriteLine(
                    $"\nDeleting job {job.JobId}, as it's marked with State {job.State} and Last Execution Status: {job.LastExecutionStatus}"
                );
                jobManagementClient.DeleteJob(JobConstants.GetJobPartition(), job.JobId);
            }
        });

    Console.WriteLine("");
    Console.WriteLine(
        "----------------------------------------------------------------------------------------------------------------------------------------"
    );
    Console.WriteLine("");
    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
}
