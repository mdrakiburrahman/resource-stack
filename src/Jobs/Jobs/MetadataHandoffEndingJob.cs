using Microsoft.AzureArcData.Sample.Jobs.JobMetadata;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Microsoft.AzureArcData.Sample.Jobs.Jobs
{
    public class BackgroundJobMeta
    {
        public const string ActionAttribute = "action";

        public BackgroundJobMeta() { }

        /// <summary>
        /// Gets or sets ActionId attribute.
        /// </summary>
        [JsonPropertyName(ActionAttribute)]
        public JObject? Action { get; set; }
    }

    public class MetadataHandoffEndingJob : JobCallback<SharedJobMetadata>
    {
        protected async Task<string> GetCustomDataByActionId(string actionId)
        {
            string jobid;
            
            // This method of getting Metadata only works for Distributed
            // Sequencers - so we wrap it in try-catch
            //
            try
            {
                jobid =
                    $"{this.JobId.Substring(0, this.JobId.LastIndexOf("-", StringComparison.Ordinal))}-{actionId.Replace(".", ":2E", StringComparison.Ordinal)}";
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Failed to get jobid: {e}");
                throw;
            }

            var backgroundjob = await this.JobManagement.GetJob(this.JobPartition, jobid);
            var actionJobject = JsonConvert.DeserializeObject<BackgroundJobMeta>(
                backgroundjob.Metadata
            );
            var actionMeta = actionJobject?.Action?.SelectToken("metadata")?.Value<string>();
            return actionMeta;
        }

        protected override async Task<JobExecutionResult> OnExecute()
        {
            // Simulate some work
            await Task.Delay(1000);

            string previousActionId = "MetadataHandoffStartingJob";
            string previousActionMeta = await GetCustomDataByActionId(previousActionId);

            // Deserialize the metadata from the previous job as SharedJobMetadata object
            var previousMetadata = JsonSerializer.Deserialize<SharedJobMetadata>(previousActionMeta);

            // Gets metadata from previous job and prints it
            //
            // String from previous job: Hello Luke, I  am your father
            // Value from previous job: 42
            //
            Console.WriteLine($"String from previous job: {previousMetadata?.SecretString}");
            Console.WriteLine($"Value from previous job: {previousMetadata?.SecretValue}");

            var executionResult = new JobExecutionResult
            {
                Status = JobExecutionStatus.Succeeded,
                Message = $"Done!",
            };

            return executionResult;
        }
    }
}
