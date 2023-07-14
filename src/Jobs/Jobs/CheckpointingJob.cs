using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Sovereign.Sample.Jobs.JobMetadata;

namespace Microsoft.Sovereign.Sample.Jobs.Jobs
{
    public class CheckpointingJob : JobCallback<CheckpointingJobMetadata>
    {
        private static ulong jobRuns = 0;

        protected override async Task<JobExecutionResult> OnExecute()
        {
            // Simulate some work
            await Task.Delay(Random.Shared.Next(200, 2000));

            var lastMetadata = Metadata;
            var currentMetadata = new CheckpointingJobMetadata
            {
                CallerName = lastMetadata.CallerName,
                MaxSteps = lastMetadata.MaxSteps ?? 0,
                CurrentStep = (lastMetadata.CurrentStep ?? 0) + 1,
            };

            var executionResult = new JobExecutionResult();

            if (currentMetadata.CurrentStep > currentMetadata.MaxSteps)
            {
                executionResult.Status = JobExecutionStatus.Succeeded;
                executionResult.Message = $"Hello {Metadata?.CallerName}! CheckpointingJob succeeded! JobNumber: {Interlocked.Increment(ref jobRuns)} after {currentMetadata.CurrentStep} steps.";
            }
            else
            {
                executionResult.Status = JobExecutionStatus.Postponed;
                executionResult.NextMetadata = JsonSerializer.Serialize(currentMetadata);
                executionResult.Message = $"Hello {Metadata?.CallerName}! CheckpointingJob rescheduled itself! JobNumber: {Interlocked.Increment(ref jobRuns)} after {currentMetadata.CurrentStep}/{currentMetadata.MaxSteps} steps.";
                executionResult.NextExecutionTime = DateTime.UtcNow;
            }

            return executionResult;
        }
    }
}
