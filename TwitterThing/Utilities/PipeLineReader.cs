using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TwitterThing.Utilities
{
    /// <summary>
    /// Pipe implementation from https://devblogs.microsoft.com/dotnet/system-io-pipelines-high-performance-io-in-net/
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static class PipeLineReader
    {
        public static Task ProcessLinesAsync(Stream stream, Action<string> OnProcessLine, CancellationToken token)
        {
            var pipe = new Pipe();
            Task writing = FillPipeAsync(stream, pipe.Writer, token);
            Task reading = ReadPipeAsync(pipe.Reader, OnProcessLine, token);

            return Task.WhenAll(reading, writing);
        }

        private static async Task FillPipeAsync(Stream stream, PipeWriter writer, CancellationToken token)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    int bytesRead = await stream.ReadAsync(memory, token);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    break;
                }

                FlushResult result = await writer.FlushAsync(token);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            writer.Complete();
        }

        private static async Task ReadPipeAsync(PipeReader reader, Action<string> OnProcessLine, CancellationToken token)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync(token);

                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    position = buffer.PositionOf((byte)'\n');

                    if (position != null)
                    {
                        OnProcessLine.Invoke(Encoding.UTF8.GetString(buffer.Slice(0, position.Value)));
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null);

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            reader.Complete();
        }
    }
}
