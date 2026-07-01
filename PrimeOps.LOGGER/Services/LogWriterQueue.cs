using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace PrimeOps.LOGGER.Services
{
    /// <summary>
    /// Single-consumer channel that serializes all disk writes,
    /// eliminating file lock contention from multiple threads/assemblies.
    /// </summary>
    public sealed class LogWriterQueue : IAsyncDisposable
    {
        private readonly Channel<(string filePath, string line)> _channel = Channel.CreateUnbounded<(string, string)>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });

        private readonly Task _consumer;
        private readonly CancellationTokenSource _cts = new();

        public LogWriterQueue()
        {
            _consumer = Task.Run(ConsumeAsync);
        }

        public void Enqueue(string filePath, string line) => _channel.Writer.TryWrite((filePath, line));

        private async Task ConsumeAsync()
        {
            await foreach (var (filePath, line) in _channel.Reader.ReadAllAsync(_cts.Token))
            {
                try
                {
                    await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
                }
                catch { /* swallow — logger must never throw */ }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _channel.Writer.Complete();
            await _consumer;
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
