using PrimeOps.LOGGER.Entities;
using PrimeOps.LOGGER.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.LOGGER.Static
{
    /// <summary>
    /// Static façade. Usage:
    ///   Log.Info.WriteLog("message", userId, LogAssembly.BLL);
    ///   Log.Error.WriteLog("message", userId, LogAssembly.API, exception);
    /// </summary>
    public static class Log
    {
        private static IFileLogger? _info;
        private static IFileLogger? _warn;
        private static IFileLogger? _error;

        public static IFileLogger Info => _info ?? throw new InvalidOperationException("Logger not initialized. Call Log.Initialize().");
        public static IFileLogger Warn => _warn ?? throw new InvalidOperationException("Logger not initialized. Call Log.Initialize().");
        public static IFileLogger Error => _error ?? throw new InvalidOperationException("Logger not initialized. Call Log.Initialize().");

        /// <summary>Called once at startup (from DI extension or manually).</summary>
        internal static void Initialize(IFileLogger info, IFileLogger warn, IFileLogger error)
        {
            _info = info;
            _warn = warn;
            _error = error;
        }
    }
}
