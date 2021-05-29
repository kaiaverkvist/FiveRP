using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    struct Payload
    {
        public string username;
        public string text;
        public string channel;
    }

    public class SlackClient
    {
        private readonly Uri _uri;

        /// <summary>
        /// Initializes a new instance of the SlackClient class.
        /// </summary>
        /// <param name="urlWithAccessToken"></param>
        public SlackClient(string urlWithAccessToken)
        {
            _uri = new Uri(urlWithAccessToken);
        }

        /// <summary>
        /// Posts a message to the slack uri specified
        /// </summary>
        /// <param name="payloadJson"></param>
        public void PostMessage(string payloadJson)
        {
            Encoding encoding = new UTF8Encoding();


            using (WebClient client = new WebClient())
            {
                // Set the payload
                NameValueCollection data = new NameValueCollection { ["payload"] = payloadJson };
                try
                {
                    var response = client.UploadValues(_uri, "POST", data);
                    string responseText = encoding.GetString(response);
                    if (responseText != "ok")
                    {
                        Console.WriteLine("SLACK: Something bad happened - " + responseText);
                    }
                }
                catch (WebException ex)
                {
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("SLACK: slack_url is not set properly");
                    }
                }
            }
        }
    }
}