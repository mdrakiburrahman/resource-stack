using Microsoft.WindowsAzure.ResourceStack.Common.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureArcData.Sample.Common.Constants
{
    public class JobConstants
    {
        public const string sqlInstanceName = "MY_OLD_SQL2019"; // MSSQLSERVER, MY_OLD_SQL2019

        public static string GetJobPartition()
        {
            return StorageUtility.EscapeStorageKey($"{sqlInstanceName}".ToUpperInvariant());
        }

        public const string backendEnvVarName = "JOB_BACKEND";
        public const string jobTableName = "arcJobDefinitions";
        public const string queueTablePrefix = "arcJobTriggers";
        public const string DefaultJobTimeoutKey = "Microsoft.WindowsAzure.ResourceStack.BackgroundJobs.DefaultJobTimeout";
        public const int DefaultJobTimeoutValueInSeconds = 5;
    }
}
