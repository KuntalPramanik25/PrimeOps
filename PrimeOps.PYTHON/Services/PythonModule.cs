using System;
using System.Collections.Generic;
using System.Text;
using Python.Runtime;
using PrimeOps.PYTHON.Entities;
using System.Text.Json;

namespace PrimeOps.PYTHON.Services
{
    public sealed class PythonModule : IDisposable
    {
        private readonly dynamic _module;
        private readonly Py.GILState _gil;
        private bool _disposed;

        internal PythonModule(string modulePath)
        {
            _gil = Py.GIL();
            try
            {
                _module = Py.Import(modulePath);
            }
            catch
            {
                _gil.Dispose();
                throw;
            }
        }

        public PythonResult Call(string functionName, params object[] args)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            try
            {
                PyObject pyFunc = _module.GetAttr(functionName);
                PyObject[] pyArgs = args.Select(a => a.ToPython()).ToArray();

                string json = pyFunc.Invoke(pyArgs).ToString()
                              ?? throw new InvalidOperationException("Python returned null.");

                return Parse(json);
            }
            catch (PythonException pex) { return PythonResult.Fail($"Python error: {pex.Message}"); }
            catch (Exception ex) { return PythonResult.Fail($"Error: {ex.Message}"); }
        }

        private static PythonResult Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            bool success = root.GetProperty("success").GetBoolean();
            string message = root.GetProperty("message").GetString() ?? string.Empty;

            JsonElement? data = null;
            if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind != JsonValueKind.Null)
            {
                // Clone so it lives beyond the using block
                data = dataEl.Clone();
            }

            return success
                ? PythonResult.Ok(data, message)
                : PythonResult.Fail(message);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _gil.Dispose();
        }
    }

    public sealed class PythonModuleBuilder
    {
        private readonly List<string> _parts = new();

        public PythonModuleBuilder Add(string part)
        {
            if (!string.IsNullOrWhiteSpace(part))
                _parts.Add(part);

            return this;
        }

        public override string ToString()
        {
            return string.Join(".", _parts);
        }

        public string Build() => ToString();
    }
}
