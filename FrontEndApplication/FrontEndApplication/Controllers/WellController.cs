using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FrontEndApplication.Controllers
{
    public class WellController : Controller
    {
        // Create service bus connection
        const string ServiceBusConnectionString = "Endpoint=sb://wellprototype.servicebus.windows.net/;SharedAccessKeyName=master;SharedAccessKey=2uWqgYUl+0PZerFo4qrVPLj1pOiaZGUHDDnXc8I8Umg=";
        const string QueueName = "wellqueue";
        static IQueueClient queueClient;
        const string StorageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=bbwelldata;AccountKey=fK2i08pNdKOB6nHQYwvvS42EJBegLRlHvqz3IBKsxMkVhkfjmNQJMLzZjFJ71Mnf5+G67t4ajR6usvHLNJVilA==;EndpointSuffix=core.windows.net";

        // Portal page
        public IActionResult Index()
        {
            return View();
        }

        // Portal page after a message is send
        public IActionResult SendMessage()
        {
            Random rnd = new Random();
            int numberOfMessages = rnd.Next(1, 20);

            MainAsync(numberOfMessages).GetAwaiter().GetResult();
            ViewBag.Result = "A total of " + numberOfMessages + " messages have been send!";
            return View("Index");
        }

        private static async Task MainAsync(int numberOfMessages)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Send messages.
            await SendMessagesAsync(numberOfMessages);

            await queueClient.CloseAsync();
        }

        // Send and upload messages
        private static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            Random rnd = new Random();
            // Create storage container for the incoming messages
            string subDomain = rnd.Next(1000000, 9999999).ToString();

            // Create a blob container to save the message in
            await CreateBlobContainer(subDomain);

            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageId = rnd.Next(10000, 99999).ToString();

                    // Save the image to the blob storage (Don't need to await this)
                    SaveToBlobAsync(subDomain, messageId);

                    // Generate the message for the service bus queue
                    Message message = GenerateQueueMessage(subDomain, messageId, numberOfMessagesToSend);

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);

                    //await Task.Delay(5000);
                }
            }
            catch
            {
                Console.WriteLine("Something went wrong.");
            }
        }

        // Generate the contents of the message
        private static Message GenerateQueueMessage(string subDomain, string messageId, int numberOfMessagesToSend)
        {
            string messageBody = subDomain + ";" + messageId + ".jpeg" + ";" + numberOfMessagesToSend.ToString();
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));
            Console.WriteLine("Message body; " + messageBody);
            return message;
        }

        // Add one file to the blob
        private static async Task SaveToBlobAsync(string subDomain, string messageId)
        {
            // Rename the file so that the filename equals the messageId
            string file = GetRandomFile();
            string newFileName = "./Resources/" + messageId + ".jpeg";
            System.IO.File.Move(file, newFileName);

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageAccountConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(subDomain);

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(messageId + ".jpeg");

            // Create or overwrite the "myblob" blob with contents from a local file.
            await blockBlob.UploadFromFileAsync(newFileName);
        }

        private static async Task CreateBlobContainer(string subDomain)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageAccountConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(subDomain);

            //Create a new container, if it does not exist
            await container.CreateIfNotExistsAsync();
        }

        // Select a random file from the ./Resources/ folder
        private static string GetRandomFile()
        {
            var rnd = new Random();
            string path = "./Resources/";
            var file = Directory.GetFiles(path, "*.jpeg")
                                 .OrderBy(x => rnd.Next())
                                 .Take(1)
                                 .FirstOrDefault();
            return file;
        }
    }
}