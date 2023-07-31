namespace Microsoft.AzureArcData.Sample.Common.Settings
{
    /// <summary>
    /// The job retry options.
    /// </summary>
    public class JobOptions
    {
        /// <summary>
        /// The jobs retry setting name.
        /// </summary>
        public const string Jobs = "Jobs";

        /// <summary>
        /// gets or sets the default retry settings.
        /// </summary>
        public JobSettings DefaultSettings { get; set; }

        /// <summary>
        /// Gets or sets the number of partitions in the jobs trigger queue.
        /// </summary>
        public int NumPartitionsInJobTriggersQueue { get; set; } = 20;

        /// <summary>
        /// Gets or sets the number of partitions in the job definition table.
        /// </summary>
        public int NumPartitionsInJobDefinitionsTable { get; set; } = 2;

        /// <summary>
        /// gets or sets the number of workers per processor count.
        /// </summary>
        public int NumWorkersPerProcessorCount { get; set; } = 12;
    }
}
