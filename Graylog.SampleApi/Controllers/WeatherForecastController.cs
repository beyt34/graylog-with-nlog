using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog.Fluent;

namespace Graylog.SampleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;

            _logger.LogTrace("ctor LogTrace...");
            Thread.Sleep(100);

            _logger.LogDebug("ctor LogDebug...");
            Thread.Sleep(100);

            _logger.LogInformation("ctor LogInformation...");
            Thread.Sleep(100);

            _logger.LogWarning("ctor LogWarning...");
            Thread.Sleep(100);

            _logger.LogError("ctor LogError...");
            Thread.Sleep(100);

            _logger.LogCritical("ctor LogCritical...");
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Get Start");

            var rng = new Random();
            var array = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();

            _logger.LogInformation("Get End");

            return array;
        }
    }
}