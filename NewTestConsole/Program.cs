using System;
using System.IO;
using VoiceRecorder.Audio;

namespace NewTestConsole
{
    internal static class Program
    {
        private static IAudioRecorder recorder;
        private static float lastPeak;
        private static string waveFileName;

        private static void Main()
        {
            RecorderViewModel(new AudioRecorder());

            var command = string.Empty;
            while (command != "break")
            {
                command = Console.ReadLine();
                switch (command)
                {
                    case "mon":
                        BeginMonitoring(0);
                        break;
                    case "start":
                        BeginRecording();
                        break;
                    case "stop":
                        Stop();
                        break;
                    default:
                        Stop();
                        return;
                }
            }

            Console.ReadKey();
        }

        private static void RecorderViewModel(IAudioRecorder rec)
        {
            recorder = rec;
            recorder.Stopped += (sender, e) => { new VoiceRecorderState(waveFileName, null); };
            SampleAggregator.MaximumCalculated +=
                (sender, e) =>
                {
                    lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
                    Console.WriteLine($"Voice level -> {CurrentInputLevel} | Time -> {RecordedTime}");
                };
        }

        private static string RecordedTime
        {
            get
            {
                var current = recorder.RecordedTime;
                return $"{current.Minutes:D2}:{current.Seconds:D2}.{current.Milliseconds:D3}";
            }
        }

        private static void BeginMonitoring(int recordingDevice)
        {
            recorder.BeginMonitoring(recordingDevice);
        }

        private static void BeginRecording()
        {
            waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            recorder.BeginRecording(waveFileName);
        }

        private static void Stop()
        {
            recorder.Stop();
        }

        public static double MicrophoneLevel
        {
            get { return recorder.MicrophoneLevel; }
            set { recorder.MicrophoneLevel = value; }
        }

        public static bool ShowWaveForm
            =>
                recorder.RecordingState == RecordingState.Recording ||
                recorder.RecordingState == RecordingState.RequestedStop;

        // multiply by 100 because the Progress bar's default maximum value is 100
        public static float CurrentInputLevel => lastPeak*100;

        public static SampleAggregator SampleAggregator => recorder.SampleAggregator;
    }
}