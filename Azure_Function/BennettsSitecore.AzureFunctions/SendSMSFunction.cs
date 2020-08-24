using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BennettsSitecore.AzureFunctions
{
    public static class SendSMSFunction
    {
        [FunctionName("SendSMSFunction")]
        public static async System.Threading.Tasks.Task RunAsync([QueueTrigger("sendsmsqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            try
            {
                

                myQueueItem = myQueueItem.Trim().Replace("\"", "");
                int mobIndex = myQueueItem.LastIndexOf("||");
                int queueItemLength = myQueueItem.Length;
                var strMobileNo = myQueueItem.Substring(mobIndex, queueItemLength - mobIndex);
                var body = myQueueItem.Substring(0, mobIndex);
                var arrMobNo = strMobileNo.Split(":", StringSplitOptions.RemoveEmptyEntries);
                var mobileNo = arrMobNo?[1];
                string fromMobileNumber = Helper.GetEnvironmentVariable("Sinch_From_Number");
                string stringPayload = ("\n{\n\"from\": \"" + fromMobileNumber + "\",\n\"to\": [ \"" + mobileNo + "\" ],\n\"body\": \"" + body + "\"\n  }");
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), Helper.GetEnvironmentVariable("Sinch_Api_Url")))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {Helper.GetEnvironmentVariable("Sinch_Api_Authentication_Key")}");
                        request.Content = new StringContent(stringPayload);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);
                    }
                }
            }
            catch(Exception ex)
            {
                log.LogInformation("Some Error Occured: " + ex);
            }
        }
    }
}
