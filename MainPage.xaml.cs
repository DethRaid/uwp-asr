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

        public MainPage() {
            this.InitializeComponent();
        }

        private void availableTopics_TextChanged(object sender, TextChangedEventArgs e) {
            var box = sender as TextBox;
            Debug.WriteLine(box.Text);
            var text = box.Text;
            topicsList.Clear();
            topicsList.AddRange(text.Split(','));
        }

        private void topics_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void setWords_Click(object sender, RoutedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(RecognizerPage), topicsList);
        }
    }
}
