using System;
using System.Diagnostics;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Task_Recognition {
    /// <summary>
    /// Implements functionality for the page that recognizes topics
    /// </summary>
    public sealed partial class RecognizerPage : Page {
        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
        private App app;

        public RecognizerPage() {
            InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            Debug.WriteLine("Navageted to page");
            app = Application.Current as App;

            listView.ItemsSource = app.getTopicsList();

            dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;

            if(app.getKeywords().Count > 0) {   // The speech recognition stuff likes to throw exceptions if you give it an empty list
                startSpeechRecognition();
            }
        }

        /// <summary>
        /// Starts a speech recognition session that can recognize the topics on the checklist and possibly related words, if that functionality is available
        /// </summary>
        /// Also adds the right callbacks to the speech recognizer
        private async void startSpeechRecognition() {
            var constraint = new SpeechRecognitionListConstraint(app.getKeywords());

            if(speechRecognizer?.State == SpeechRecognizerState.Capturing) {
                await speechRecognizer.ContinuousRecognitionSession.StopAsync();
            }

            speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(constraint);
            var speechCompilationResult = await speechRecognizer.CompileConstraintsAsync();
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += speechResultCallback;

            if(speechRecognizer.State == SpeechRecognizerState.Idle) {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                Debug.WriteLine("Started speech recognition session");
            }
        }

        private async void speechResultCallback(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) {
            if(args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High) {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    var recognizedText = args.Result.Text;
                    var keywordMapping = app.getKeywordMapping();
                    var topic = keywordMapping[recognizedText];
                    Debug.WriteLine("Recognized text " + recognizedText + " from topic " + topic);

                    checkMatchingTopics(topic);
                });

            } else {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Debug.WriteLine("Recognized word '" + args.Result.Text + "' with a low confidence"));
            }
        }

        /// <summary>
        /// Examines all the checklist items in the UI, checking off any that match the given topic
        /// </summary>
        /// <param name="topic">The topic to check CheckBoxes for</param>
        private void checkMatchingTopics(string topic) {
            foreach(var x in listView.Items) {
                var item = x as CheckBox;
                var textInItem = (string)item.Content;
                Debug.WriteLine("Text box has text " + textInItem);
                if(textInItem == topic) {
                    Debug.WriteLine("Recognized a word on our list!");
                    item.IsChecked = true;
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(InputSingleTopicPage));
        }
    }
}
