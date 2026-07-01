using Microsoft.AspNetCore.Mvc;
using PrimeOps.ENTITY;
using PrimeOps.LOGGER.Entities;
using PrimeOps.LOGGER.Static;
using PrimeOps.PYTHON.Interfaces;
using PrimeOps.PYTHON.Services;
using Python.Runtime;

namespace PrimeOps.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController (IPythonEngineManager engine) : ControllerBase
    {

        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet]
        [Route("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            Log.Info.WriteLog("Payment processed", 1, LogAssembly.BLL);

            string module = new PythonModuleBuilder().Add(PrimeOpsPythonEngine.ModuleName).Add(PrimeOpsPythonModules.TestOperations).Add(PrimeOpsPythonLibrary.Calculator).Build();
            using var calc = engine.Import(module);

            var subtractRes = calc.Call(PrimeOpsPythonFunctions.Subtract, 10, 4);
            var addRes = calc.Call("add", 3.5, 2.1);

            double result = addRes.GetData<double>();


            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
