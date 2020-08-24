using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Extensions.Logging;

namespace BennettsSitecore.AzureFunctions
{
    public static class SendDMFunction
    {
        [FunctionName("SendDMFunction")]
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Helper.GetEnvironmentVariable("AzureWebJobsStorage"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("senddirectmailqueue");
            if(await queue.ExistsAsync())
            {
                var batch = await queue.GetMessagesAsync(30);
                foreach (CloudQueueMessage myQueueItem in batch)
                {
                    try
                    {
                         dynamic RetrievedMesageInQueue = myQueueItem.AsString;
                        RetrievedMesageInQueue = RetrievedMesageInQueue.Trim().Replace("\"", "");
                        var IndexfileName = RetrievedMesageInQueue.IndexOf("||");
                        var MACampaign = RetrievedMesageInQueue.Substring(0, IndexfileName);
                        var arrFileNames = MACampaign.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        Helper.addRecordToAzureFileShare(RetrievedMesageInQueue, $"{arrFileNames?[1]}_{ DateTime.Today.ToString("yyyyMMdd")}", log);
                         await queue.DeleteMessageAsync(myQueueItem);
                    }
                    catch(Exception ex)
                    {
                        log.LogInformation($"some error occured "+ ex);
                    }
                }
            }
            else
            {
                log.LogInformation($"Queue does not exist");
            }
        }
    }
}
