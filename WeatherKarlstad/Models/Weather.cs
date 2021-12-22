using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace WeatherKarlstad.Models
{
	public class Temperature
	{
		public float current;
		public float feelsLike;
		public float tempMin;
		public float tempMax;
	}

	// Daily weather in the API uses a different data structure for temperatures
	public class DailyTemp
	{
		public float day;
		public float min;
		public float max;
		public float night;
		public float eve;
		public float morn;
	}

	public class DailyWeather
	{
		public DailyTemp temp = new DailyTemp();
		public int clouds;
		public string weather;
		public float downfall;
		public float windSpeed;
		public int windDir;
	}

	public class HourlyWeather
	{
		public Temperature temp = new Temperature();
		public int clouds;
		public string weather;
		public float downfall;
		public float windSpeed;
		public int windDir;
	}


	/* 
	 * Main object for this model
	 * Contains data about the current weather as well as two arrays of objects
	 * representing the hourly forecast and the daily forecast
	 */
	public class Weather
	{
		public Temperature temp = new Temperature();
		public int currentClouds;
		public string currentWeather;
		public float currentWindSpeed;
		public int currentWindDir;

		// 48 From the number of hours retreived from the API
		public HourlyWeather[] hourly = new HourlyWeather[48];
		// 8 From the number of days retreived from the API
		public DailyWeather[] daily = new DailyWeather[8];

		public Weather()
		{
			// Initialize the arrays of objects
			for (int i = 0; i < hourly.Length; i++)
			{
				hourly[i] = new HourlyWeather();
			}
			for (int i = 0; i < daily.Length; i++)
			{
				daily[i] = new DailyWeather();
			}
		}
	}
}