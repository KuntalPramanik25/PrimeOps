using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.LOGGER.Entities
{
    public sealed record LogEntry
    {
        private const string Separator = "=========================================================";
        private const int LabelWidth = 13; // aligns all labels

        // ── Core fields ──────────────────────────────────────────────────────────
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        public LogLevel Level { get; init; }
        public LogAssembly Assembly { get; init; }
        public int UserId { get; init; } = 0;
        public string Message { get; init; } = string.Empty;
        public string? CorrelationId { get; init; }
        public string? ExceptionDetails { get; init; }
        public Dictionary<string, object>? Metadata { get; init; }

        // ── Diagnostic fields (captured at creation time) ─────────────────────
        public string MachineName { get; init; } = Environment.MachineName;
        public int ThreadId { get; init; } = Environment.CurrentManagedThreadId;
        public int ProcessId { get; init; } = Environment.ProcessId;

        // ── Rendering ─────────────────────────────────────────────────────────
        public string Format (DiagnosticInfoLevel diagnosticLevel)
        {
            bool showDiag = diagnosticLevel == DiagnosticInfoLevel.Always || (diagnosticLevel == DiagnosticInfoLevel.ErrorOnly && Level == LogLevel.Error);

            var sb = new StringBuilder();

            Field(sb, "Timestamp", TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            Field(sb, "Level", Level.ToString().ToUpper());
            Field(sb, "Assembly", Assembly.ToString());
            Field(sb, "User", UserId.ToString());

            if (!string.IsNullOrWhiteSpace(CorrelationId))
                Field(sb, "CorrelationId", CorrelationId);

            if (showDiag)
            {
                Field(sb, "Machine", MachineName);
                Field(sb, "Thread", ThreadId.ToString());
                Field(sb, "Process", ProcessId.ToString());
            }

            if (Metadata is { Count: > 0 })
                Field(sb, "Metadata", System.Text.Json.JsonSerializer.Serialize(Metadata));

            // Message block
            sb.AppendLine("Message");
            sb.AppendLine($"  {Message}");

            // Exception block (errors only)
            if (!string.IsNullOrWhiteSpace(ExceptionDetails))
            {
                sb.AppendLine("Exception");
                foreach (var line in ExceptionDetails.Split('\n'))
                    sb.AppendLine($"  {line.TrimEnd()}");
            }

            sb.AppendLine(Separator);
            sb.AppendLine(); // blank line between entries

            return sb.ToString();
        }

        // Convenience override with no diagnostics (for ToString() callers)
        public override string ToString() => Format(DiagnosticInfoLevel.Never);

        private static void Field (StringBuilder sb, string label, string value)
        {
            sb.AppendLine($"{label.PadRight(LabelWidth)}: {value}");
        }
    }
}
