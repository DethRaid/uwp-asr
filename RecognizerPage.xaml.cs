using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Task_Recognition {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecognizerPage : Page {
        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
        private List<string> topicsList;

        public RecognizerPage() {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            topicsList = e.Parameter as List<string>;
            Debug.WriteLine("Recognizing topics:");
            topicsList.ForEach(x => Debug.WriteLine(x));

            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            var constraint = new SpeechRecognitionListConstraint(topicsList);

            speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(constraint);
            var speechCompilationResult = await speechRecognizer.CompileConstraintsAsync();
            Debug.WriteLine("Created Speech Recognition stuff");

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += speechResultCallback;
            Debug.WriteLine("Registered speech callback");

            speechRecognizer.ContinuousRecognitionSession.Completed += speechSessionCompletedCallback;
            Debug.WriteLine("Registered session ended callback");

            speechRecognizer.HypothesisGenerated += hypothesisGeneratedCallback;
            Debug.WriteLine("Registered hypothesis callback");

            if(speechRecognizer.State == SpeechRecognizerState.Idle) {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                Debug.WriteLine("Started speech recognition session");
            }
        }

        private void speechSessionCompletedCallback(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args) {
            Debug.WriteLine("Speech recognition session ended. Status: " + args.Status);
        }

        private async void speechResultCallback(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) {
            Debug.WriteLine("Recognized some text");
            if(args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High) {
                var recognizedText = args.Result.Text;
                Debug.WriteLine("Recognized word " + recognizedText);

                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Debug.WriteLine("Recognized word " + args.Result.Text));

            } else {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Debug.WriteLine("Recognized word " + args.Result.Text));
            }
        }

        private void hypothesisGeneratedCallback(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args) {
            Debug.WriteLine("Recieved speech hypothesis " + args.Hypothesis.Text);
        }
    }
}
