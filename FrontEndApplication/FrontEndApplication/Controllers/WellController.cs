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
        private static readonly string serviceBusKey = System.IO.File.ReadAllText(@"C:\Users\r.d.scheele\OneDrive - Betabit\Keys\service_bus_key.txt");
        private static readonly string storageAccountKey = System.IO.File.ReadAllText(@"C:\Users\r.d.scheele\OneDrive - Betabit\Keys\storage_account_key.txt");
        // Create service bus connection
        private static readonly string serviceBusConnectionString = "Endpoint=sb://wellprototype.servicebus.windows.net/;SharedAccessKeyName=master;SharedAccessKey=" + serviceBusKey;
        private static readonly string queueName = "wellqueue";
        static IQueueClient queueClient;
        private static readonly string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=bbwelldata;AccountKey=" + storageAccountKey + ";EndpointSuffix=core.windows.net";

        // Portal page
        public IActionResult Index()
        {
            return View();
        }

        // Portal page after a message is send
        public IActionResult SendMessage(string size)
        {
            MainAsync(size).GetAwaiter().GetResult();
            ViewBag.Result = "Items with a " + size + " size have created messages and have been send to the service bus!";

            return View("Index");
        }

        private static async Task MainAsync(string messageSize)
        {
            queueClient = new QueueClient(serviceBusConnectionString, queueName);

            // Send messages.
            await SendMessagesAsync(messageSize);

            await queueClient.CloseAsync();
        }

        // Send and upload messages
        private static async Task SendMessagesAsync(string messageSize)
        {
            Random rnd = new Random();
            // Create storage container for the incoming messages
            string subDomain = rnd.Next(1000000, 9999999).ToString();

            // Create a blob container to save the message in
            await CreateBlobContainer(subDomain);

            int numberOfMessagesToSend = 0;
            int fakeCpu = 0;
            int fakeMemory = 0;


            if (messageSize == "small")
            {
                numberOfMessagesToSend = 96;
                fakeCpu = 2;
                fakeMemory = 100000000;
            }
            if (messageSize == "medium")
            {
                numberOfMessagesToSend = 48;
                fakeCpu = 5;
                fakeMemory = 200000000;
            }
            if (messageSize == "large")
            {
                numberOfMessagesToSend = 6;
                fakeCpu = 20;
                fakeMemory = 700000000;
            }


            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageId = rnd.Next(10000, 99999).ToString();

                    // Save the image to the blob storage (Don't need to await this)
                    SaveToBlobAsync(subDomain, messageId);

                    // Generate the message for the service bus queue
                    // Message format subdomain;messageId;numberOfMessagesInBatch;fakeCpuValueToBeUsed;fakeMemoryValueToBeUsed
                    Message message = GenerateQueueMessage(subDomain, messageId, numberOfMessagesToSend, fakeCpu, fakeMemory);

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
        private static Message GenerateQueueMessage(string subDomain, string messageId, int numberOfMessagesToSend, int fakeCpu, int fakeMemory)
        {
            string messageBody = subDomain + ";" + messageId + ".jpeg" + ";" + numberOfMessagesToSend.ToString() + ";" + fakeCpu.ToString() + ";" + fakeMemory.ToString();
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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);

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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);

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