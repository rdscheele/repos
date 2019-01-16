using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEndApplication.Models
{
    public class MessageObject
    {
        public MessageObject(string FullBody, string Subdomain, string MessageId, string NumberOfMessagesToSend, string FakeCpu, string FakeMemory) {
            this.FullBody = FullBody;
            this.SubDomain = SubDomain;
            this.MessageId = MessageId;
            this.NumberOfMessagesToSend = NumberOfMessagesToSend;
            this.FakeCpu = FakeCpu;
            this.FakeMemory = FakeMemory;
        }

        public string FullBody { get; set; }
        public string SubDomain { get; set; }
        public string MessageId { get; set; }
        public string NumberOfMessagesToSend { get; set; }
        public string FakeCpu { get; set; }
        public string FakeMemory { get; set; }
    }
}
