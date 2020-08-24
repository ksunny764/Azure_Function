using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BennettsSitecore.AzureFunctions
{
    public static class SendDirectMailFunction
    {
        [FunctionName("Function1")]
        public static void Run([QueueTrigger("senddirectmailqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            try
            {
                myQueueItem = myQueueItem.Trim().Replace("\"", "");
                var IndexfileName = myQueueItem.IndexOf("||");
                var MACampaign = myQueueItem.Substring(0, IndexfileName);
                var arrFileNames = MACampaign.Split(":", StringSplitOptions.RemoveEmptyEntries);
                Helper.addRecordToAzureFileShare(myQueueItem, $"{arrFileNames?[1]}_{ DateTime.Today.ToString("yyyyMMdd")}", log);
            }
            catch (Exception ex)
            {
                log.LogInformation("Some Error Occured " + ex);
            }
        }
    }
}
