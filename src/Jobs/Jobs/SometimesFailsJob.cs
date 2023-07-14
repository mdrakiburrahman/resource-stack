using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AzureArcData.Sample.Jobs.JobMetadata;

namespace Microsoft.AzureArcData.Sample.Jobs.Jobs
{
    public class SometimesFailsJob : JobCallback<SometimesFailsJobMetadata>
    {
        private static ulong jobRuns = 0;

        protected override async Task<JobExecutionResult> OnExecute()
        {
            var chanceOfFailing = Metadata?.ChanceOfFailure ?? 0.5;
            var callerName = Metadata?.CallerName ?? "Anonymous";

            JobExecutionResult executionResult;

            var runNumber = Interlocked.Increment(ref jobRuns);

            // Simulate some work
            await Task.Delay(Random.Shared.Next(200, 2000));

            if (Random.Shared.NextDouble() <= chanceOfFailing)
            {
                executionResult = new JobExecutionResult
                {
                    Status = JobExecutionStatus.Failed,
                    Message =
                        $"Hello {callerName}, SometimesFailsJob failed! JobNumber: {runNumber}"
                };
            }
            else
            {
                executionResult = new JobExecutionResult
                {
                    Status = JobExecutionStatus.Succeeded,
                    Message =
                        $"Hello {callerName}, SometimesFailsJob succeeded! JobNumber: {runNumber}"
                };
            }
            return executionResult;
        }
    }
}
