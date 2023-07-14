using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AzureArcData.Sample.Jobs.JobMetadata;

namespace Microsoft.AzureArcData.Sample.Jobs.Jobs
{
    public class AlwaysSucceedJob : JobCallback<AlwaysSucceedJobMetadata>
    {
        private static ulong jobRuns = 0;

        protected override async Task<JobExecutionResult> OnExecute()
        {
            // Simulate some work
            await Task.Delay(Random.Shared.Next(200, 2000));

            var executionResult = new JobExecutionResult
            {
                Status = JobExecutionStatus.Succeeded,
                Message = $"Hello {Metadata?.CallerName}! AlwaysSucceedJob succeeded! JobNumber: {Interlocked.Increment(ref jobRuns)}",
            };

            return executionResult;
        }
    }
}
