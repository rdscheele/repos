using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;

namespace FrontEndApplication.Controllers
{
    public class WellController : Controller
    {
        const string ServiceBusConnectionString = "Endpoint=sb://wellprototype.servicebus.windows.net/;SharedAccessKeyName=master;SharedAccessKey=2uWqgYUl+0PZerFo4qrVPLj1pOiaZGUHDDnXc8I8Umg=";
        const string QueueName = "wellqueue";
        static IQueueClient queueClient;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SendMessage()
        {
            MainAsync().GetAwaiter().GetResult();
            ViewBag.Result = "Message send!";
            return View("Index");
        }

        static async Task MainAsync()
        {
            const int numberOfMessages = 10;
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Send messages.
            await SendMessagesAsync(numberOfMessages);

            await queueClient.CloseAsync();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            Random rnd = new Random();
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}" + rnd.Next(10000, 99999);
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);

                    //await Task.Delay(5000);
                }
            }
            catch (Exception exception)
            {
                
            }
        }
    }
}