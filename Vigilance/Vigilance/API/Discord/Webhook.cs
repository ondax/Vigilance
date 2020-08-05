using System.Net;
using System;
using Newtonsoft.Json;

namespace Vigilance.API.Discord
{
    public class Webhook
    {

        private Uri _Uri;
        public Webhook(string URL)
        {
            if (!Uri.TryCreate(URL, UriKind.Absolute, out _Uri))
            {
                throw new UriFormatException();
            }
        }

        public string Post(string message)
        {
            using (WebClient wb = new WebClient())
            {
                Message msg = new Message
                {
                    content = message
                };
                wb.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                return wb.UploadString(_Uri, "POST", JsonConvert.SerializeObject(msg));
            }
        }
        public struct Message
        {
            public string content;
        }
    }
}
