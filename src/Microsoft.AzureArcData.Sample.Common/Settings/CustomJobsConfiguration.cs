using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs.Configuration;

namespace Microsoft.AzureArcData.Sample.Common.Settings
{
    /// <summary>
    /// The Custom jobs configuration.
    /// </summary>
    public class CustomJobsConfiguration : DefaultJobsConfigurationProvider
    {
        /// <summary>
        /// Gets or sets the job options.
        /// </summary>
        public JobOptions JobOptions { get; set; }

        /// <summary>
        /// Gets the default job timeout.
        /// </summary>
        public override TimeSpan DefaultJobTimeout { get; }

        /// <summary>
        /// Gets the default job retention.
        /// </summary>
        public override TimeSpan DefaultSequencerTimeout { get; }

        /// <summary>
        /// Gets the default job retention.
        /// </summary>
        public override TimeSpan DefaultJobRetention { get; }

        /// <summary>
        /// Gets the default job retention.
        /// </summary>
        public override TimeSpan DefaultSequencerRetention { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FusionJobsConfiguration"/> class.
        /// </summary>
        /// <param name="jobOptions">The job options.</param>
        public CustomJobsConfiguration(JobOptions jobOptions)
        {
            this.JobOptions = jobOptions;

            this.DefaultJobTimeout = this.JobOptions.DefaultSettings.JobTimeout;
            this.DefaultSequencerTimeout = this.JobOptions.DefaultSettings.SequencerTimeout;

            this.DefaultJobRetention = this.JobOptions.DefaultSettings.Retention;
            this.DefaultSequencerRetention = this.JobOptions.DefaultSettings.Retention;
        }

        /// <summary>
        /// Gets the number of workers in the job dispatching service.
        /// </summary>
        public override int NumWorkersInJobDispatchingService =>
            Environment.ProcessorCount * this.NumWorkersPerProcessorCount;

        /// <summary>
        /// Gets the number of workers per processor count.
        /// </summary>
        public override int NumWorkersPerProcessorCount =>
            this.JobOptions.NumWorkersPerProcessorCount;
    }
}
