using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using PrimeOps.LOGGER.Configurations;
using PrimeOps.LOGGER.Entities;
using PrimeOps.LOGGER.Helpers;
using PrimeOps.LOGGER.Interfaces;

namespace PrimeOps.LOGGER.Services
{
    public sealed class FileLogger (IOptions<LoggerOptions> options, LogWriterQueue queue, LogLevel level) : IFileLogger
    {
        private readonly LoggerOptions _opts = options.Value;
        private readonly LogLevel _level = level;

        public void WriteLog (string message, int userId, LogAssembly assembly, string? correlationId = null, Dictionary<string, object>? metadata = null)
        {
            var entry = new LogEntry
            {
                Level = _level,
                Message = message,
                UserId = userId,
                Assembly = assembly,
                CorrelationId = correlationId,
                Metadata = metadata
            };
            WriteLog(entry);
        }

        public void WriteLog (string message, int userId, LogAssembly assembly, Exception exception, string? correlationId = null, Dictionary<string, object>? metadata = null)
        {
            var entry = new LogEntry
            {
                Level = _level,
                Message = message,
                UserId = userId,
                Assembly = assembly,
                CorrelationId = correlationId,
                Metadata = metadata,
                ExceptionDetails = exception.ToString()
            };
            WriteLog(entry);
        }

        public void WriteLog (LogEntry entry)
        {
            try
            {
                var filePath = LogPathResolver.Resolve (_opts.BasePath!, _level, entry.TimeStamp, _opts.MaxFileSizeMb);

                queue.Enqueue(filePath, entry.ToString());
            }
            catch { /* logger must never throw */ }
        }
    }
}
