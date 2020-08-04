using System.Collections.Specialized;
using System.Net;
using System;

namespace Vigilance.API.Discord
{
    public class Webhook
    {
        private string _url;

        public Webhook(string url) => _url = url;

        public void Post(string message)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    NameValueCollection collection = new NameValueCollection()
                {
                    {
                       "content",
                        message
                    }
                };
                    webClient.UploadValues(_url, collection);
                }
            }
            catch (Exception e)
            {
                Log.Error("Webhook", e);
            }
        }
    }
}
