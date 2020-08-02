using System.Collections.Specialized;
using System.Net;

namespace Vigilance.API.Discord
{
    public class Webhook
    {
        private string _url;

        public Webhook(string url) => _url = url;

        public void Post(string message)
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
    }
}
