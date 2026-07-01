using PrimeOps.LOGGER.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.LOGGER.Configurations
{
    public class LoggerOptions
    {
        public const string SectionName = "FileLogger";

        /// <summary>Root path where Logs/ folder will be created.</summary>
        public string? BasePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>Max file size in MB before a new segment is created.</summary>
        public int MaxFileSizeMb { get; set; } = 10;

        /// <summary>How long to keep logs (months). Used by cleanup cron.</summary>
        public int RetentionMonths { get; set; } = 6;

        /// <summary>
        /// Controls when Machine, Thread, and Process are written to the log.
        /// </summary>
        public DiagnosticInfoLevel DiagnosticInfo { get; set; } = DiagnosticInfoLevel.ErrorOnly;
    }
}
