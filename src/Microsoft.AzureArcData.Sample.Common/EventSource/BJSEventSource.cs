using Microsoft.WindowsAzure.ResourceStack.Common.EventSources;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.AzureArcData.Sample.Common.EventSource
{
    public class BJSEventSource : IBackgroundJobsEventSource
    {
        public void JobCritical(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName, string jobPartition, string jobId, string message, string exception, string organizationId, string activityVector, string realPuid,
            string altSecId, string additionalProperties)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                jobPartition,
                jobId,
                message,
                exception,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void JobDebug(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName, string jobPartition, string jobId, string message, string exception, string organizationId, string activityVector, string realPuid,
            string altSecId, string additionalProperties)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                jobPartition,
                jobId,
                message,
                exception,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void JobError(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName, string jobPartition, string jobId, string message, string exception, string organizationId, string activityVector, string realPuid,
            string altSecId, string additionalProperties)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                jobPartition,
                jobId,
                message,
                exception,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void JobOperation(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName, string jobPartition, string jobId, string message, string exception, string organizationId, string activityVector, string realPuid,
            string altSecId, string additionalProperties)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                jobPartition,
                jobId,
                message,
                exception,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void JobWarning(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName, string jobPartition, string jobId, string message, string exception, string organizationId, string activityVector, string realPuid,
            string altSecId, string additionalProperties)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                jobPartition,
                jobId,
                message,
                exception,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void JobDispatchingError(string operationName, string jobPartition, string jobId, string message, string exception, string subscriptionId, string correlationId, string principalOid,
            string principalPuid, string tenantId, string organizationId, string activityVector, string realPuid, string altSecId, string additionalProperties)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                jobPartition,
                jobId,
                message,
                exception,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }


        public void JobDefinition(string jobPartition, string jobId, string version, string callback, string location, string locationsAffinity, string flags, string state, string executionState,
            string startTime, string endTime, int repeatCount, long repeatInterval, string repeatUnit, string repeatSchedule, int currentRepeatCount, int retryCount, long retryInterval,
            string retryUnit, int currentRetryCount, int currentExecutionCount, string timeout, string retention, string nextExecutionTime, string lastExecutionTime, string lastExecutionStatus,
            string createdTime, string changedTime, string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, int totalSucceededCount,
            int totalCompletedCount, int totalFailedCount, int totalFaultedCount, int totalPostponedCount, string parentJobCompletionTrigger, string organizationId, string activityVector,
            string realPuid, string altSecId, string additionalProperties)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                "JobDefinition",
                jobPartition,
                jobId,
                string.Empty,
                string.Empty,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
            InternalWrite(false, $"\tVersion: {version} | Callback: {callback} | Location: {location} | LocationAffinity: {locationsAffinity}\n" +
                $"\tFlags: {flags} | State: {state} | ExecutionState: {executionState} | StartTime: {startTime} | EndTime: {endTime}\n" +
                $"\tRepeatCount: {repeatCount} | RepeatInterval: {repeatInterval} | RepeatUnit: {repeatUnit} | RepeatSchedule: {repeatSchedule} | CurrentRepeatCount: {currentRepeatCount}\n" +
                $"\tRetryCount: {retryCount} | RetryInterval: {retryInterval} | RetryUnit: {retryUnit} | CurrentRetryCount {currentRetryCount} | CurrentExecutionCount: {currentExecutionCount}\n" +
                $"\tTimeout: {timeout} | Retention: {retention} | NextExecutionTime: {nextExecutionTime} | LastExecutionTime: {lastExecutionTime} | LastExecutionStatus: {lastExecutionStatus}\n" +
                $"\tCreatedTime: {createdTime} | ChangedTime: {changedTime}\n" +
                $"\tTotalSucceededCount: {totalSucceededCount} | TotalCompletedCount: {totalCompletedCount} | TotalFailedCount: {totalFailedCount} | TotalFaultedCount: {totalFaultedCount} | TotalPostponedCount: {totalPostponedCount} | ParentJobCompletionTrigger: {parentJobCompletionTrigger}\n");
        }


        public void JobHistory(string jobPartition, string jobId, string callback, string startTime, string endTime, string executionTimeInMilliseconds, string executionDelayInMilliseconds,
            string executionIntervalInMilliseconds, string executionStatus, string executionMessage, string executionDetails, string nextExecutionTime, string subscriptionId, string correlationId,
            string principalOid, string principalPuid, string tenantId, string dequeueCount, string advanceVersion, string triggerId, string messageId, string state, string organizationId,
            string activityVector, string realPuid, string altSecId, string additionalProperties, string jobDurabilityLevel)
        {
            JobGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                "JobHistory",
                jobPartition,
                jobId,
                executionMessage,
                string.Empty,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
            InternalWrite(false,
                $"\tCallback: {callback} | StartTime: {startTime} | EndTime: {endTime}\n" +
                $"\tExecutionTimeInMilliseconds: {executionTimeInMilliseconds} | executionDelayInMilliseconds: {executionDelayInMilliseconds} | executionIntervalInMilliseconds: {executionIntervalInMilliseconds}\n" +
                $"\tExecutionStatus: {executionStatus} | ExecutionDetails: {executionDetails} | NextExecutionTime: {nextExecutionTime}\n" +
                $"\tDequeueCount: {dequeueCount} | AdvanceVersion: {advanceVersion} | TriggerId: {triggerId} | MessageId: {messageId}");
        }


        public void StorageOperation(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName, string accountName,
            string resourceType, string resourceName, string clientRequestId, string operationStatus, long durationInMilliseconds, string exceptionMessage, int requestsStarted,
            int requestsCompleted, int requestsTimedout, string requestsDetails, string organizationId, string activityVector, long ingressBytes, long egressBytes, string realPuid,
            string altSecId, string additionalProperties)
        {
            StorageGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                accountName,
                resourceType,
                resourceName,
                clientRequestId,
                "",
                durationInMilliseconds,
                0,
                exceptionMessage,
                "",
                "",
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
            InternalWrite(false, $"\toperationStatus: {operationStatus} | durationInMilliseconds: {durationInMilliseconds} | ingressBytes: {ingressBytes} | egressBytes: {egressBytes}\n" +
                $"\trequestsStarted: {requestsStarted} | requestsCompleted: {requestsCompleted} | requestsTimedout: {requestsTimedout} | requestsDetails: {requestsDetails}");
        }

        public void StorageRequestEndWithClientFailure(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName,
            string accountName, string resourceType, string resourceName, string clientRequestId, string serviceRequestId, long durationInMilliseconds, int httpStatusCode, string exceptionMessage,
            string errorCode, string errorMessage, string organizationId, string activityVector, string realPuid, string altSecId, string additionalProperties)
        {
            StorageGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                accountName,
                resourceType,
                resourceName,
                clientRequestId,
                serviceRequestId,
                durationInMilliseconds,
                httpStatusCode,
                exceptionMessage,
                errorCode,
                errorMessage,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void StorageRequestEndWithServerFailure(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName,
            string accountName, string resourceType, string resourceName, string clientRequestId, string serviceRequestId, long durationInMilliseconds, int httpStatusCode, string exceptionMessage,
            string errorCode, string errorMessage, string organizationId, string activityVector, string realPuid, string altSecId, string additionalProperties)
        {
            StorageGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                accountName,
                resourceType,
                resourceName,
                clientRequestId,
                serviceRequestId,
                durationInMilliseconds,
                httpStatusCode,
                exceptionMessage,
                errorCode,
                errorMessage,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void StorageRequestEndWithSuccess(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName,
            string accountName, string resourceType, string resourceName, string clientRequestId, string serviceRequestId, long durationInMilliseconds, int httpStatusCode, string exceptionMessage,
            string errorCode, string errorMessage, string organizationId, string activityVector, string realPuid, string altSecId, string additionalProperties)
        {
            StorageGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                accountName,
                resourceType,
                resourceName,
                clientRequestId,
                serviceRequestId,
                durationInMilliseconds,
                httpStatusCode,
                exceptionMessage,
                errorCode,
                errorMessage,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }

        public void StorageRequestStart(string subscriptionId, string correlationId, string principalOid, string principalPuid, string tenantId, string operationName,
            string accountName, string resourceType, string resourceName, string clientRequestId, string serviceRequestId, long durationInMilliseconds, int httpStatusCode, string exceptionMessage,
            string errorCode, string errorMessage, string organizationId, string activityVector, string realPuid, string altSecId, string additionalProperties)
        {
            StorageGenericLogging(
                subscriptionId,
                correlationId,
                principalOid,
                principalPuid,
                tenantId,
                operationName,
                accountName,
                resourceType,
                resourceName,
                clientRequestId,
                serviceRequestId,
                durationInMilliseconds,
                httpStatusCode,
                exceptionMessage,
                errorCode,
                errorMessage,
                organizationId,
                activityVector,
                realPuid,
                altSecId,
                additionalProperties
            );
        }


        private void StorageGenericLogging(
            string subscriptionId,
            string correlationId,
            string principalOid,
            string principalPuid,
            string tenantId,
            string operationName,
            string accountName,
            string resourceType,
            string resourceName,
            string clientRequestId,
            string serviceRequestId,
            long durationInMilliseconds,
            int httpStatusCode,
            string exceptionMessage,
            string errorCode,
            string errorMessage,
            string organizationId,
            string activityVector,
            string realPuid,
            string altSecId,
            string additionalProperties,
            [CallerMemberName] string? caller_name = null
            )
        {
            if (string.IsNullOrEmpty(caller_name) || string.IsNullOrWhiteSpace(caller_name))
                caller_name = "unkown";

            string customerInfo = $"subscriptionId: {subscriptionId} | tenantId: {tenantId} | principalOid: {principalOid} | principalPuid: {principalPuid}";
            string storageInfo = $"accountName: {accountName} | correlationId: {correlationId} | operationName: {operationName} | resourceType: {resourceType} | resourceName: {resourceName}";
            string requestInfo = $"clientRequestId: {clientRequestId} | serverRequestId: {serviceRequestId} | durationInMilliseconds: {durationInMilliseconds} | httpStatusCode: {httpStatusCode}";
            string errorInfo = $"exceptionMessage: {exceptionMessage} | errorCode: {errorCode} | errorMessage: {errorMessage}";
            string additionalInfo = $"organizationId: {organizationId} | activityVector: {activityVector} | realPuid: {realPuid} | altSecId: {altSecId} | additionalProperties: {additionalProperties} | EventSourceMethod: {caller_name}";
            bool isError = caller_name == "StorageRequestEndWithClientFailure" || caller_name == "StorageRequestEndWithServerFailure";

            InternalWrite(isError, $"Storage Log\n\tStorageInfo: {storageInfo}\n\tCustomerInfo: {customerInfo}\n\tAdditionalInfo: {additionalInfo}\n\tErrorInfo: {errorInfo}\n\tRequestInfo: {requestInfo}");
        }


        private void JobGenericLogging(
            string subscriptionId,
            string correlationId,
            string principalOid,
            string principalPuid,
            string tenantId,
            string operationName,
            string jobPartition,
            string jobId,
            string message,
            string exception,
            string organizationId,
            string activityVector,
            string realPuid,
            string altSecId,
            string additionalProperties,
            [CallerMemberName] string? caller_name = null
            )
        {
            if (string.IsNullOrEmpty(caller_name) || string.IsNullOrWhiteSpace(caller_name))
                caller_name = "unkown";

            string customerInfo = $"subscriptionId: {subscriptionId} | tenantId: {tenantId} | principalOid: {principalOid} | principalPuid: {principalPuid}";
            string jobInfo = $"jobId: {jobId} | correlationId: {correlationId} | operationName: {operationName} | jobPartition: {jobPartition}";
            string additionalInfo = $"organizationId: {organizationId} | activityVector: {activityVector} | realPuid: {realPuid} | altSecId: {altSecId} | additionalProperties: {additionalProperties} | EventSourceMethod: {caller_name}";
            bool isError = false;
            if (caller_name == "JobError" || caller_name == "JobWarning")
                isError = true;

            InternalWrite(isError, $"Job Log\n\tJobInfo: {jobInfo}\n\tCustomerInfo: {customerInfo}\n\tAdditionalInfo: {additionalInfo}\n\tMessage: {message}\n\tException: {exception}");
        }

        private void LogUnknown([CallerMemberName] string? caller_name = null)
        {
            InternalWrite(false, $"Logging call to {caller_name}");
        }

        private void InternalWrite(bool isError, string message)
        {

            Console.ForegroundColor = isError ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine($"[{DateTime.Now}] {message}");
            Console.ResetColor();
        }
    }
}
