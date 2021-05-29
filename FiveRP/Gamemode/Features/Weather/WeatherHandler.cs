using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;
using GTANetworkServer;
using Newtonsoft.Json.Linq;

namespace FiveRP.Gamemode.Features.Weather
{
    public class WeatherHandler : Script, IDisposable
    {
        //private enum WeatherType { Clear, Extrasunny, Clouds, Overcast, Rain, Clearing, Thunder, Smog, Foggy, Xmas, Snowlight, Blizzard }
        private enum WeatherType { Extrasunny, Clear, Clouds, Smog, Foggy, Overcast, Rain, Thunder, Clearing, Neutral, Snow, Blizzard, Snowlight, Xmas}

        private const string WeatherCity = "Los Angeles,US";
        private const string WeatherApiKey = "d66d54714e235bf30560c6b3f7897be5";

        private Timer _weatherTimer;

        public WeatherHandler()
        {
            API.onResourceStart += OnResourceStart;
            API.onResourceStop += OnResourceStop;
        }

        // Starts the weather handle timer that makes sure the weather retrieval gets ran.
        private void OnResourceStart()
        {
            _weatherTimer = new Timer(5 * 60 * 1000);
            _weatherTimer.Elapsed += HandleWeather;
            _weatherTimer.AutoReset = true;
            _weatherTimer.Enabled = true;

            HandleWeather(null, null);
        }

        private void OnResourceStop()
        {
            _weatherTimer.Dispose();
        }

        private void HandleWeather(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var weather = GetWeatherType(GetWeatherId(WeatherApiKey, WeatherCity));
            API.setWeather((int)weather);
        }

        /// <summary>
        /// Gets the weather type based on Id.
        /// </summary>
        /// <returns>WeatherType</returns>
        private WeatherType GetWeatherType(int weatherId)
        {
            if (weatherId >= 200 && weatherId < 300)
            {
                return WeatherType.Thunder;
            }
            if (weatherId == 300 || weatherId == 301)
            {
                return WeatherType.Clearing;
            }
            if (weatherId >= 300 && weatherId < 600)
            {
                return WeatherType.Rain;
            }
            if ((new[] { 600, 601, 611, 612, 615, 616, 620, 621 }).Contains(weatherId))
            {
                return WeatherType.Snowlight;
            }
            if ((new[] { 602, 622 }).Contains(weatherId))
            {
                return WeatherType.Blizzard;
            }
            if (weatherId == 741)
            {
                return WeatherType.Foggy;
            }
            if (weatherId == 711)
            {
                return WeatherType.Smog;
            }
            if (weatherId == 800)
            {
                return WeatherType.Extrasunny;
            }
            if (weatherId > 800 && weatherId < 804)
            {
                return WeatherType.Clear;
            }
            if (weatherId == 804)
            {
                return WeatherType.Clouds;
            }
            return WeatherType.Clear;
        }

        /// <summary>
        /// Gets the weather id from the openweathermap.org API using the given API key and region.
        /// </summary>
        /// <returns>Weather Id</returns>
        private int GetWeatherId(string apiKey, string weatherCity)
        {
            try
            {
                var url = $"http://api.openweathermap.org/data/2.5/weather?q={weatherCity}&APPID={apiKey}";
                var w = WebRequest.Create(url);
                var response = w.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseJsoNstring = reader.ReadToEnd();
                    response.Close();
                    dynamic data = JObject.Parse(responseJsoNstring);

                    return (int)(data.weather[0].id);
                }
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            _weatherTimer.Dispose();
        }
    }
}