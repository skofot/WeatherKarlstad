using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WeatherKarlstad.Models;

namespace WeatherKarlstad.Controllers
{
    public class WeatherController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("DailyWeather", "Weather");
        }

        public ActionResult HourlyWeather()
        {
            var weather = new Weather();

            FetchAndFillWeatherObject(weather,
                "https://api.openweathermap.org/data/2.5/onecall?lat=59,3793&lon=13,5036&appid=97eb8642c68ac2918163ed44434e6cad&units=metric");

            RoundWeatherData(weather);

            return View(weather);
        }

        public ActionResult DailyWeather()
        {
            var weather = new Weather();

            FetchAndFillWeatherObject(weather,
                "https://api.openweathermap.org/data/2.5/onecall?lat=59,3793&lon=13,5036&appid=97eb8642c68ac2918163ed44434e6cad&units=metric");

            RoundWeatherData(weather);

            return View(weather);
        }

        public void FetchAndFillWeatherObject(Weather weather, string uri)
        {
            // Fetch data with the API and wait for the response
            var t = Task.Run(() =>
                GetWeatherData(uri));
            t.Wait();

            // Weather data is stored as a JSON object so we parse it
            var weatherInfo = JObject.Parse(t.Result);

            // Get current weather data
            weather.temp.current = (float)weatherInfo["current"]["temp"];
            weather.temp.feelsLike = (float)weatherInfo["current"]["feels_like"];
            weather.currentClouds = (int)weatherInfo["current"]["clouds"];
            weather.currentWeather = (string)weatherInfo["current"]["weather"][0]["main"];
            weather.currentWindDir = (int)weatherInfo["current"]["wind_deg"];
            weather.currentWindSpeed = (float)weatherInfo["current"]["wind_speed"];

            // Fill weather data for the available 48 hours
            for (int i = 0; i < weather.hourly.Length; i++)
            {
                // Make a variable for easier read
                var hour = weatherInfo["hourly"][i];

                // Max and min temperature for hourly forecasts are not available through the API
                weather.hourly[i].temp.current = (float)hour["temp"];
                weather.hourly[i].temp.feelsLike = (float)hour["feels_like"];
                weather.hourly[i].clouds = (int)hour["clouds"];
                weather.hourly[i].weather = (string)hour["weather"][0]["main"];
                weather.hourly[i].windSpeed = (float)hour["wind_speed"];
                weather.hourly[i].windDir = (int)hour["wind_deg"];

                // Check downfall for the hour. Rain and snow are both handled as downfall
                if (hour["snow"] != null)
                    weather.hourly[i].downfall = (float)hour["snow"]["1h"];
                else if (hour["rain"] != null)
                    weather.hourly[i].downfall = (float)hour["rain"]["1h"];
            }

            // Fill weather data for the available coming 8 days (today included)
            for (int i = 0; i < weather.daily.Length; i++)
            {
                // Make a variable for easier read
                var day = weatherInfo["daily"][i];

                // Since we have matching structures here we can convert the entire DailyTemp object at once
                JObject jTemp = day["temp"] as JObject;
                weather.daily[i].temp = jTemp.ToObject<DailyTemp>();
                
                weather.daily[i].clouds = (int)day["clouds"];
                weather.daily[i].weather = (string)day["weather"][0]["main"];
                weather.daily[i].windSpeed = (float)day["wind_speed"];
                weather.daily[i].windDir = (int)day["wind_deg"];

                // Check downfall for the entire day. Rain and snow are both handled as downfall
                if (day["snow"] != null)
                    weather.daily[i].downfall = (float)day["snow"];
                else if (day["rain"] != null)
                    weather.daily[i].downfall = (float)day["rain"];
            }

        }

        // Round away unneccessary decimals in the data
        private void RoundWeatherData(Weather weather)
        {
            weather.currentWindSpeed = (float)Math.Round((double)weather.currentWindSpeed, 1);
            weather.temp.feelsLike = (float)Math.Round((double)weather.temp.feelsLike, 1);
            weather.temp.current = (float)Math.Round((double)weather.temp.current, 1);

            foreach (HourlyWeather hour in weather.hourly)
            {
                hour.temp.current = (float)Math.Round((double)hour.temp.current, 1);
                hour.temp.feelsLike = (float)Math.Round((double)hour.temp.feelsLike, 1);
                hour.windSpeed = (float)Math.Round((double)hour.windSpeed, 1);
                hour.downfall = (float)Math.Round((double)hour.downfall, 1);
            }

            foreach (DailyWeather day in weather.daily)
            {
                day.temp.max = (float)Math.Round((double)day.temp.max, 1);
                day.temp.min = (float)Math.Round((double)day.temp.min, 1);
                day.windSpeed = (float)Math.Round((double)day.windSpeed, 1);
                day.downfall = (float)Math.Round((double)day.downfall, 1);
            }
        }

        // Fetch API with GET  
        static async Task<string> GetWeatherData(string uri)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.GetAsync(uri);
                if (result.IsSuccessStatusCode)
                    response = await result.Content.ReadAsStringAsync();
            }
            return response;
        }

    }
}