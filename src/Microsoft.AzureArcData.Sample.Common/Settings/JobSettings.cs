namespace Microsoft.AzureArcData.Sample.Common.Settings
{
    using System;

    /// <summary>
    /// The retry settings.
    /// </summary>
    public class JobSettings
    {
        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        public int Count { get; set; } = 5;

        /// <summary>
        /// Gets or sets the max execution count.
        /// </summary>
        public int MaxExecutionCount { get; set; } = 4;

        /// <summary>
        /// Gets or sets the max job lifetime.
        /// </summary>
        public TimeSpan MaxJobLifetime { get; set; } = TimeSpan.Parse("01:30:00");

        /// <summary>
        /// Gets or sets the min interval.
        /// </summary>
        public TimeSpan MinInterval { get; set; } = TimeSpan.Parse("00:03:30");

        /// <summary>
        /// Gets or sets the max interval.
        /// </summary>
        public TimeSpan MaxInterval { get; set; } = TimeSpan.Parse("00:10:00");

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public TimeSpan StartTime { get; set; } = TimeSpan.Parse("00:00:00");

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        public TimeSpan EndTime { get; set; } = TimeSpan.Parse("01:30:00");

        /// <summary>
        /// Gets or sets the Job/Action timeout.
        /// </summary>
        public TimeSpan JobTimeout { get; set; } = TimeSpan.Parse("00:15:00");

        /// <summary>
        /// Gets or sets the Sequencer timeout.
        /// </summary>
        public TimeSpan SequencerTimeout { get; set; } = TimeSpan.Parse("01:00:00");

        /// <summary>
        /// Gets or sets the retention.
        /// </summary>
        public TimeSpan Retention { get; set; } = TimeSpan.Parse("24:00:00");

        /// <summary>
        /// The default constructor.
        /// </summary>
        public JobSettings() { }
    }
}
