using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrimeOps.PYTHON.Entities;
using PrimeOps.PYTHON.Interfaces;
using Python.Runtime;

namespace PrimeOps.PYTHON.Services
{
    public sealed class PythonEngineManager : IPythonEngineManager
    {
        private readonly PythonSettings _settings;
        private readonly ILogger<PythonEngineManager> _logger;
        private bool _initialized;
        private bool _disposed;

        public PythonEngineManager (IOptions<PythonSettings> settings, ILogger<PythonEngineManager> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            Initialize();
        }

        // ── Init ───────────────────────────────────────────────────────────────

        private void Initialize()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_settings.PythonDll))
                    Runtime.PythonDLL = _settings.PythonDll;

                if (!string.IsNullOrWhiteSpace(_settings.PythonHome))
                    PythonEngine.PythonHome = _settings.PythonHome;

                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();

                if (_settings.AdditionalPaths.Count > 0)
                {
                    using (Py.GIL())
                    {
                        dynamic sys = Py.Import("sys");
                        foreach (var path in _settings.AdditionalPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
                            sys.path.append(path);
                    }
                }

                _initialized = true;
                _logger.LogInformation("Python engine initialised.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialise Python engine.");
                if (_settings.ThrowOnInitFailure) throw;
            }
        }

        // ── Primary API ────────────────────────────────────────────────────────

        /// <summary>
        /// Import a Python module. GIL is held until the returned PythonModule is disposed.
        ///
        ///     using var calc = _engine.Import("primeops_python_engine.test_operations.calculator");
        ///     var res  = calc.Call("subtract", 10, 4);
        ///     var data = res.GetData&lt;Dictionary&lt;string, double&gt;&gt;();
        ///     // res.Data / res.Success / res.Message
        /// </summary>
        public PythonModule Import(string modulePath)
        {
            EnsureInitialized();
            return new PythonModule(modulePath);   // acquires GIL inside constructor
        }

        // ── Low-level GIL helpers ──────────────────────────────────────────────

        public void ExecuteInGil(Action<PyModule> action)
        {
            EnsureInitialized();
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();
                action(scope);
            }
        }

        public T ExecuteInGil<T>(Func<PyModule, T> func)
        {
            EnsureInitialized();
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();
                return func(scope);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void EnsureInitialized()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_initialized)
                throw new InvalidOperationException("Python engine is not initialised.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_initialized)
            {
                try { PythonEngine.Shutdown(); }
                catch (Exception ex) { _logger.LogWarning(ex, "Error shutting down Python engine."); }
            }
        }
    }
}