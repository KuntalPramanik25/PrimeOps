using PrimeOps.PYTHON.Entities;
using PrimeOps.PYTHON.Services;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.PYTHON.Interfaces
{
    public interface IPythonEngineManager : IDisposable
    {
        /// <summary>
        /// Imports a Python module and returns a scoped handle.
        /// The GIL is held until the returned PythonModule is disposed.
        ///
        /// Always use inside a `using` block:
        ///     using var calc = _engine.Import("primeops_python_engine.test_operations.calculator");
        ///     var res = calc.Call("subtract", 10, 4);
        /// </summary>
        PythonModule Import(string modulePath);

        /// <summary>
        /// Low-level GIL scope for raw PythonNet operations.
        /// </summary>
        void ExecuteInGil(Action<PyModule> action);

        /// <summary>
        /// Low-level GIL scope with return value.
        /// </summary>
        T ExecuteInGil<T>(Func<PyModule, T> func);
    }
}
