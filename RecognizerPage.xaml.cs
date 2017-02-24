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
        private Dictionary<string, RichEditBox> topicBoxes;

        public RecognizerPage() {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            topicsList = e.Parameter as List<string>;
            Debug.WriteLine("Recognizing topics:");

            topicBoxes = new Dictionary<string, RichEditBox>();

            topicsList.ForEach(topic => {
                Debug.WriteLine(topic);
                RichEditBox item = new RichEditBox();
                item.Document.SetText(Windows.UI.Text.TextSetOptions.None, topic);

                listBox.Items.Add(item);
                topicBoxes.Add(topic, item);
            });

            dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;

            var constraint = new SpeechRecognitionListConstraint(topicsList);

            speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(constraint);
            var speechCompilationResult = await speechRecognizer.CompileConstraintsAsync();
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += speechResultCallback;
            speechRecognizer.ContinuousRecognitionSession.Completed += speechSessionCompletedCallback;
            speechRecognizer.HypothesisGenerated += hypothesisGeneratedCallback;

            if(speechRecognizer.State == SpeechRecognizerState.Idle) {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                Debug.WriteLine("Started speech recognition session");
            }
        }

        private void speechSessionCompletedCallback(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args) {
            Debug.WriteLine("Speech recognition session ended. Status: " + args.Status);
        }

        private async void speechResultCallback(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High) {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    var recognizedText = args.Result.Text;
                    Debug.WriteLine("Recognized word '" + recognizedText + "'");

                    foreach (var x in listBox.Items) {
                        var item = x as RichEditBox;
                        var textInItem = "";
                        item.Document.GetText(Windows.UI.Text.TextGetOptions.None, out textInItem);
                        Debug.WriteLine("Text box has text " + textInItem);
                        if (textInItem == recognizedText) {
                            Debug.WriteLine("Recognized a word on our list!");
                        }
                    }
                });

            } else {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Debug.WriteLine("Recognized word '" + args.Result.Text + "' with a low confidence"));
            }
        }

        private async void hypothesisGeneratedCallback(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args) {
            Debug.WriteLine("Recieved speech hypothesis " + args.Hypothesis.Text);

            if(args.Hypothesis.Text != "...") { 
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    var recognizedText = args.Hypothesis.Text;
                    var listItem = topicBoxes[recognizedText];
                });
            }
        }
    }
}
