using System.Reflection;
using System.Configuration;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.AzureArcData.Sample.Jobs.Jobs;
using Microsoft.AzureArcData.Sample.Common;
using Microsoft.AzureArcData.Sample.Common.EventSource;
using Microsoft.AzureArcData.Sample.Common.Constants;

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
JobDispatcherClient jobDispatcherClient;
switch (backend)
{
    case Backend.cosmosdb:

        jobDispatcherClient = new JobDispatcherClient(
            documentServiceEndpoint: new Uri(
                ConfigurationManager.AppSettings["documentServiceEndpoint"]
                    ?? "https://localhost:8081"
            ),
            documentAuthorizationKey: ConfigurationManager.AppSettings["documentAuthorizationKey"]
                ?? "KeyMissing",
            executionAffinity: "global",
            eventSource: new BJSEventSource(),
            encryptionUtility: null
        );

        break;

    case Backend.sqlserver:

        jobDispatcherClient = new SqlJobDispatcherClient(
            databaseConnectionString: ConfigurationManager.AppSettings["sqlServerConnectionString"],
            executionAffinity: "global",
            eventSource: new BJSEventSource(),
            encryptionUtility: null
        );

        break;

    default:
        throw new Exception($"This demo doesn't have support for {backend} just yet!");
}

jobDispatcherClient.RegisterJobCallback(typeof(AlwaysSucceedJob));
jobDispatcherClient.RegisterJobCallback(typeof(SometimesFailsJob));
jobDispatcherClient.RegisterJobCallback(typeof(CheckpointingJob));
jobDispatcherClient.ProvisionSystemConsistencyJob().Wait();

Console.WriteLine("Starting Job Dispatcher");
jobDispatcherClient.Start();
Console.WriteLine("Waiting for infinity");

Task.Delay(Timeout.InfiniteTimeSpan).Wait();
