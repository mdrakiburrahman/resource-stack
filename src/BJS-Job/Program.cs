using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Configuration;
using Microsoft.AzureArcData.Sample.Common;
using System.Reflection;
using Microsoft.AzureArcData.Sample.Jobs.Jobs;
using Microsoft.AzureArcData.Sample.Common.EventSource;

JobDispatcherClient jobDispatcherClient = new JobDispatcherClient(
    documentServiceEndpoint: new Uri(ConfigurationManager.AppSettings["documentServiceEndpoint"] ?? "https://localhost:8081"),
    documentAuthorizationKey: ConfigurationManager.AppSettings["documentAuthorizationKey"] ?? "KeyMissing", executionAffinity: "global",
    eventSource: new BJSEventSource(),
    encryptionUtility: null
    );

jobDispatcherClient.RegisterJobCallback(typeof(AlwaysSucceedJob));
jobDispatcherClient.RegisterJobCallback(typeof(SometimesFailsJob));
jobDispatcherClient.RegisterJobCallback(typeof(CheckpointingJob));
jobDispatcherClient.ProvisionSystemConsistencyJob().Wait();

Console.WriteLine("Starting Job Dispatcher");
jobDispatcherClient.Start();
Console.WriteLine("Waiting for infinity");

Task.Delay(Timeout.InfiniteTimeSpan).Wait();