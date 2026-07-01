using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PrimeOps.PYTHON.Entities
{
    public sealed class PythonResult
    {
        public JsonElement? Data { get; init; }   // kept as raw JsonElement
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;

        public static PythonResult Ok(JsonElement? data, string message = "") => new() { Data = data, Success = true, Message = message };

        public static PythonResult Fail(string message) => new() { Data = null, Success = false, Message = message };

        /// <summary>
        /// Deserialise Data into any type:
        ///     res.GetData;double&gt;()
        ///     res.GetData;MyEntity&gt;()
        ///     res.GetData;List&lt;MyEntity&gt;&gt;()
        ///     res.GetData;Dictionary&lt;string, object&gt;&gt;()
        /// </summary>
        public T? GetData<T>()
        {
            if (Data is null) return default;
            return JsonSerializer.Deserialize<T>(Data.Value.GetRawText());
        }
    }
}
