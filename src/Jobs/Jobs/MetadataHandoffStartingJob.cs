using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AzureArcData.Sample.Jobs.JobMetadata;

namespace Microsoft.AzureArcData.Sample.Jobs.Jobs
{
    public class MetadataHandoffStartingJob : JobCallback<SharedJobMetadata>
    {
        protected override async Task<JobExecutionResult> OnExecute()
        {
            // Simulate some work
            await Task.Delay(1000);

            Metadata.SecretString = "Hello Luke, I  am your father";
            Metadata.SecretValue = 42;

            var executionResult = new JobExecutionResult
            {
                Status = JobExecutionStatus.Succeeded,
                NextMetadata = JsonSerializer.Serialize(Metadata),
                Message =
                    $"Left some metadata for caller to be used by other jobs: {JsonSerializer.Serialize(Metadata)}",
            };

            return executionResult;
        }
    }
}
