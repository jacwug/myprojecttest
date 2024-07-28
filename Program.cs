using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;

class Program
{
    // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
    static string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
    static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

    async static Task Main(string[] args)
    {
        var filepath = "katiesteve.wav";
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";

        var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Create an audio stream from a wav file or from the default microphone
        using (var audioConfig = AudioConfig.FromWavFileInput(filepath))
        {
            // Create a conversation transcriber using audio stream input
            using (var conversationTranscriber = new ConversationTranscriber(speechConfig, audioConfig))
            {
                conversationTranscriber.Transcribing += (s, e) =>
                {
                    // Console.WriteLine($"TRANSCRIBING: Text={e.Result.Text}");
                };

                conversationTranscriber.Transcribed += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"TRANSCRIBED: Text={e.Result.Text} Speaker ID={e.Result.SpeakerId}");
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be transcribed.");
                    }
                };

                conversationTranscriber.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                        stopRecognition.TrySetResult(0);
                    }

                    stopRecognition.TrySetResult(0);
                };

                conversationTranscriber.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    stopRecognition.TrySetResult(0);
                };

                await conversationTranscriber.StartTranscribingAsync();

                // Waits for completion. Use Task.WaitAny to keep the task rooted.
                Task.WaitAny(new[] { stopRecognition.Task });

                await conversationTranscriber.StopTranscribingAsync();
            }
        }
    }
}