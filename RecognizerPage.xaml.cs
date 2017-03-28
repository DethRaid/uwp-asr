﻿using System;
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
        private HashSet<string> topicsList;
        private Dictionary<string, RichEditBox> topicBoxes;

        public RecognizerPage() {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            topicsList = e.Parameter as HashSet<string>;
            Debug.WriteLine("Recognizing topics:");

            topicBoxes = new Dictionary<string, RichEditBox>();

            var totalHeight = 0.0;

            listView.ItemsSource = topicsList;

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

                    foreach (var x in listView.Items) {
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

                    listView.Items.Remove(listItem);

                    if(listView.Items.Count == 0) {
                        successMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                });
            }
        }

        private void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            // TODO: Open the "enter a new topic" thing
        }
    }
}
