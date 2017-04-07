using System;
using System.Diagnostics;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Task_Recognition {
    /// <summary>
    /// This page lets people input a new topic to be recognized
    /// </summary>
    public sealed partial class InputSingleTopicPage : Page {
        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;

        public InputSingleTopicPage() {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;

            speechRecognizer = new SpeechRecognizer();
            await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.HypothesisGenerated += speechHypothesisCallback;
            speechRecognizer.StateChanged += onSpeechRecognitionEnded;
            //speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(3);
            //speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(0.5);

            await speechRecognizer.RecognizeAsync();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if(speechRecognizer.State == SpeechRecognizerState.Capturing || speechRecognizer.State == SpeechRecognizerState.Processing) {
                Debug.WriteLine("Ending speech recognition session because page was navigated away from");
                await speechRecognizer.StopRecognitionAsync();
            }
        }

        private async void speechHypothesisCallback(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args) {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => spokenText.Text = args.Hypothesis.Text);
        }

        private async void onSpeechRecognitionEnded(SpeechRecognizer recognizer, SpeechRecognizerStateChangedEventArgs e) {
            if(e.State == SpeechRecognizerState.Idle) {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listeningOutputTextBox.Text = "Done listening");
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            addCurrentTopic();

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(RecognizerPage));
        }

        private void saveAndAddAnother_Click(object sender, RoutedEventArgs e) {
            addCurrentTopic();

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(InputSingleTopicPage));
        }

        private void addCurrentTopic() {
            var topic = spokenText.Text;

            var app = Application.Current as App;
            app.addTopic(topic);
        }
    }
}
