using PrimeOps.LOGGER.Entities;
using PrimeOps.LOGGER.Interfaces;
using PrimeOps.LOGGER.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.LOGGER.Interfaces
{

    public interface IFileLogger
    {
        void WriteLog(string message, int userId, LogAssembly assembly, string? correlationId = null, Dictionary<string, object>? metadata = null);

        void WriteLog(string message, int userId, LogAssembly assembly, Exception exception, string? correlationId = null, Dictionary<string, object>? metadata = null);

        void WriteLog(LogEntry entry);
    }
}