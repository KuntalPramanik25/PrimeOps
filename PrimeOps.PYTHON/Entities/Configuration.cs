using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.PYTHON.Entities
{
    public class PythonSettings
    {
        public const string Section = "Python";

        /// <summary>Full path to the Python home (e.g. C:\Python311 or /usr/local)</summary>
        public string PythonHome { get; set; } = string.Empty;

        /// <summary>Full path to python311.dll / libpython3.11.so</summary>
        public string PythonDll { get; set; } = string.Empty;

        /// <summary>
        /// Additional paths appended to sys.path.
        /// Put your .pyc package / site-packages directory here.
        /// </summary>
        public List<string> AdditionalPaths { get; set; } = [];

        /// <summary>Log Python initialization errors without crashing the host</summary>
        public bool ThrowOnInitFailure { get; set; } = true;
    }
}
