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

            foreach(var topic in app.getTopicsList()) {
                CheckBox box = new CheckBox();
                box.Content = topic;
                topicsChecklist.Items.Add(box);
            }

            dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;

            if(app.getKeywords().Count > 0) {   // The speech recognition stuff likes to throw exceptions if you give it an empty list
                startSpeechRecognition();
            } else {
                Debug.WriteLine("No keywords provided");
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
            } else {
                Debug.WriteLine("Speech recognizer is not idle, attemptint to reboot");
                await speechRecognizer.ContinuousRecognitionSession.StopAsync();
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async void speechResultCallback(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) {
            if(args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High) {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    var recognizedText = args.Result.Text;
                    var keywordMapping = app.getKeywordMapping();
                    var topic = keywordMapping[recognizedText];
                    Debug.WriteLine("Recognized text " + recognizedText + " from topic " + topic);

                    checkCheckboxWithContent(topic);
                });

            } else {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Debug.WriteLine("Recognized word '" + args.Result.Text + "' with a low confidence"));
            }
        }

        /// <summary>
        /// Examines all the checklist items in the UI, checking off any that match the given topic
        /// </summary>
        /// <param name="topic">The topic to check CheckBoxes for</param>
        /// <param name="toggle">If true, toggle the CheckBox. If false, set the CheckBox to checked</param>
        private void checkCheckboxWithContent(string topic, bool toggle = false) {
            foreach(var x in topicsChecklist.Items) {
                var item = x as CheckBox;
                var textInItem = (string)item.Content;

                if(textInItem == topic) {
                    if(toggle) {
                        item.IsChecked = !item.IsChecked;
                    } else {
                        item.IsChecked = true;
                    }
                }
            }
        }

        private void addTopicButton_Click(object sender, RoutedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(InputSingleTopicPage));
        }

        private void topicsChecklist_ItemClick(object sender, ItemClickEventArgs e) {
            Debug.WriteLine("Clicked on item " + e.ClickedItem);
            var itemText = (string)(e.ClickedItem as CheckBox).Content;
            checkCheckboxWithContent(itemText, true);
        }

        private void clearSelectedButton_Click(object sender, RoutedEventArgs e) {
            foreach(var item in topicsChecklist.Items) {
                (item as CheckBox).IsChecked = false;
            }
        }
    }
}
