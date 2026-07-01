using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeOps.PYTHON.Entities;
using PrimeOps.PYTHON.Interfaces;
using PrimeOps.PYTHON.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PrimeOps.PYTHON.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddPythonService (this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PythonSettings>(
                configuration.GetSection(PythonSettings.Section));

            services.AddSingleton<IPythonEngineManager, PythonEngineManager>();

            return services;
        }
    }
}
