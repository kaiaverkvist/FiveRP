using System.Net;
using Newtonsoft.Json.Linq;

namespace FiveRP.Gamemode.Library
{
    public class Authentication
    {
        public static AuthResult Auth(string email, string password, string socialclub)
        {
            // unless stated otherwise, the server should use the test api to authenticate.
            var apiurl = "https://api.test.fiverp.net/authenticate";

            // if the server is in production mode, use the live API.
            if(Config.GetKeyInt("#production") == 1)
            {
                apiurl = "https://api.fiverp.net/authenticate";
            }
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                client.Headers.Set("X-Authorization", Config.GetKeyString("#ucp_apikey"));

                string dataString = $"email={email}&password={password}&socialclub={socialclub}";

                var response = client.UploadString(apiurl, dataString);

                var result = response;
                var resultObj = JObject.Parse(result);

                try
                {
                    return new AuthResult((bool)resultObj["success"], (bool)resultObj["application"], (bool)resultObj["enabled"], (string)resultObj["message"]);
                } catch
                {
                    return new AuthResult(false, false, false, "Login failed");
                }
            }
        }
    }

    public class AuthResult
    {
        public bool Success;
        public bool Application;
        public bool Enabled;
        public string Message;

        public AuthResult(bool v1, bool v2, bool v3, string v4)
        {
            Success = v1;
            Application = v2;
            Enabled = v3;
            Message = v4;
        }
    }
}