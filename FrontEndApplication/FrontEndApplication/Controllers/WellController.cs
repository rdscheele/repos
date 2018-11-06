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

        // Portal page
        public IActionResult Index()
        {
            return View();
        }

        // Portal page after a message is send
        public IActionResult SendMessage()
        {
            MainAsync().GetAwaiter().GetResult();
            ViewBag.Result = "Message send!";
            return View("Index");
        }

        private static async Task MainAsync()
        {
            const int numberOfMessages = 10;
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Send messages.
            await SendMessagesAsync(numberOfMessages);

            await queueClient.CloseAsync();
        }

        private static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            Random rnd = new Random();
            // Create storage container for the incoming messages
            string subDomain = rnd.Next(1000000, 9999999).ToString();
            //CreateStorageContainer(subDomain);

            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageId = rnd.Next(10000, 99999).ToString();
                    string messageBody = $"Message {i} ;" + messageId;
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                    Console.WriteLine(messageBody);

                    //SaveToBlob(subDomain, messageId);

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

        // Add one file to the blob
        private static void SaveToBlob(string subDomain, string messageId)
        {
            // Rename the file so that the filename equals the messageId
            string file = GetRandomFile();
            string newFileName = "./Resources/" + messageId + ".jpeg";
            System.IO.File.Move(file, newFileName);

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("fK2i08pNdKOB6nHQYwvvS42EJBegLRlHvqz3IBKsxMkVhkfjmNQJMLzZjFJ71Mnf5+G67t4ajR6usvHLNJVilA==");

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(subDomain);

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(newFileName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = System.IO.File.OpenRead(@"path\myfile"))
            {
                //blockBlob.UploadFromStream(fileStream);
            }

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

        // Creates a storage container
        private static void CreateStorageContainer(string subDomain)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("fK2i08pNdKOB6nHQYwvvS42EJBegLRlHvqz3IBKsxMkVhkfjmNQJMLzZjFJ71Mnf5+G67t4ajR6usvHLNJVilA==");

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //the container for this is companystyles
            CloudBlobContainer container = blobClient.GetContainerReference(subDomain);

            //Create a new container, if it does not exist
            container.CreateIfNotExistsAsync();
        }
    }
}