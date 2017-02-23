using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Task_Recognition {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private List<string> topicsList = new List<string>();
        private SpeechRecognizer speechRecognizer = new SpeechRecognizer();

        public MainPage() {
            this.InitializeComponent();
        }

        private void setWords_Click(object sender, RoutedEventArgs e) {
            buildGrammar();
        }

        private async void buildGrammar() {
            var constraint = new SpeechRecognitionListConstraint(topicsList);

            speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(constraint);
            await speechRecognizer.CompileConstraintsAsync();
            SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();
            await speechRecognizer.RecognizeWithUIAsync();
            var messageDialog = new Windows.UI.Popups.MessageDialog(speechRecognitionResult.Text, "Text spoken");
            await messageDialog.ShowAsync();
        }

        private void availableTopics_TextChanged(object sender, TextChangedEventArgs e) {
            var box = sender as TextBox;
            Debug.WriteLine(box.Text);
            var text = box.Text;
            topicsList.AddRange(text.Split(','));
        }

        private void topics_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }
    }
}
