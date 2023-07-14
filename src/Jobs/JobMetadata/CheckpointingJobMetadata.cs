namespace Microsoft.Sovereign.Sample.Jobs.JobMetadata
{
    public class CheckpointingJobMetadata
    {
        public string? CallerName { get; set; }

        public int? MaxSteps { get; set; }

        public int? CurrentStep { get; set; }
    }
}
