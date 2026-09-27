using NAudio.Wave;
using WledSRServer.Audio;

Console.WriteLine("Starting ParecLoopbackCapture prototype via IAudioCapture (3s)...");

IAudioCapture capture = new ParecLoopbackCapture();
var chunkCount = 0;
var peak = 0.0;
var sw = System.Diagnostics.Stopwatch.StartNew();

capture.DataAvailable += (_, e) =>
{
    chunkCount++;
    var samples = e.BytesRecorded / 4;
    double sumSquares = 0;
    for (int i = 0; i < samples; i++)
    {
        var sample = BitConverter.ToSingle(e.Buffer, i * 4);
        sumSquares += sample * sample;
    }
    var rms = samples > 0 ? Math.Sqrt(sumSquares / samples) : 0;
    peak = Math.Max(peak, rms);

    if (chunkCount % 20 == 0)
        Console.WriteLine($"chunk {chunkCount,4}  bytes={e.BytesRecorded,4}  rms={rms:F4}  state={capture.CaptureState}");
};

Exception? stoppedException = null;
var stoppedSignal = new ManualResetEventSlim(false);
capture.RecordingStopped += (_, e) =>
{
    stoppedException = e.Exception;
    stoppedSignal.Set();
};

capture.StartRecording();
Console.WriteLine($"WaveFormat: {capture.WaveFormat}");

Thread.Sleep(3000);
capture.StopRecording();

// give RecordingStopped a moment to fire if it hasn't already
stoppedSignal.Wait(TimeSpan.FromSeconds(2));

Console.WriteLine();
Console.WriteLine($"Total chunks: {chunkCount} in {sw.Elapsed.TotalSeconds:F2}s (expected ~{sw.Elapsed.TotalSeconds * 100:F0})");
Console.WriteLine($"Peak RMS observed: {peak:F4}");
Console.WriteLine($"Final CaptureState: {capture.CaptureState}");
Console.WriteLine($"RecordingStopped exception: {(stoppedException == null ? "none" : stoppedException.Message)}");

capture.Dispose();
