using System;
using System.Text;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;
using Microsoft.Extensions.Logging;

namespace BennettsSitecore.AzureFunctions
{
    internal class Helper
    {
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static void addRecordToAzureFileShare(string myQueueItem, string fileName, ILogger log)
        {
            try
            {
                string valueLine = string.Empty;
                string clientHeader = string.Empty;
                var searcResult = new string[10];
                StringBuilder builder = new StringBuilder();
                StringBuilder headerBuilder = new StringBuilder();
                StringBuilder valueBuilder = new StringBuilder();
                string[] arrQueueItem = myQueueItem.Split("||", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < arrQueueItem.Length; i++)
                {
                    var arrHeader = arrQueueItem[i]?.Split(":", StringSplitOptions.RemoveEmptyEntries);
                    if (!string.IsNullOrEmpty(arrHeader[0]) && !arrHeader[0].Trim().Equals("CampaignName"))
                    {
                        string header = arrHeader[0].Replace(",", "-");
                        string value = arrHeader.Length > 1 ? arrHeader[1].Replace(",", "-") : ""; //Replace comma with hyphen in values
                        headerBuilder.Append(header + ",");
                        valueBuilder.Append(value + ",");
                    }
                }
                var headerLastIndex = headerBuilder.ToString().LastIndexOf(",");
                if (headerLastIndex >= 0)
                    clientHeader = headerBuilder.ToString().Remove(headerLastIndex, 1) + Environment.NewLine;
                builder.Append(clientHeader);
                var valueLastIndex = valueBuilder.ToString().LastIndexOf(",");
                if (valueLastIndex >= 0)
                    valueLine = valueBuilder.ToString().Remove(valueLastIndex, 1);
                builder.Append(valueLine);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetEnvironmentVariable("AzureWebJobsStorage"));
                CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
                CloudFileShare fileShare = fileClient.GetShareReference(GetEnvironmentVariable("AzureStorageFileShare"));
                string existingFiletxt = "";
                if (fileShare.Exists())
                {
                    CloudFileDirectory rootDirectory = fileShare.GetRootDirectoryReference();
                    if (rootDirectory.Exists())
                    {
                        CloudFileDirectory customDirectory = rootDirectory.GetDirectoryReference(GetEnvironmentVariable("AzureStorageFileDirectory"));
                        if (customDirectory.Exists())
                        {
                            CloudFile file = customDirectory.GetFileReference(fileName + ".csv");
                            if (file.Exists())
                            {
                                existingFiletxt = file.DownloadTextAsync().Result;
                                string existingHeader = existingFiletxt.Split(new[] { Environment.NewLine }, StringSplitOptions.None)?[0];
                                string existingValue = existingFiletxt.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[1];
                                if (existingHeader + Environment.NewLine == clientHeader)
                                {
                                    customDirectory.GetFileReference(fileName + ".csv").UploadText(existingFiletxt + Environment.NewLine + valueLine);
                                }
                                else
                                {
                                    existingFiletxt = clientHeader + existingValue;
                                    customDirectory.GetFileReference(fileName + ".csv").UploadText(existingFiletxt + Environment.NewLine + valueLine.ToString());
                                }
                            }
                            else
                            {
                                customDirectory.GetFileReference(fileName + ".csv").UploadText(builder.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogInformation("Some error occured " + ex);
            }
        }
    }
}
