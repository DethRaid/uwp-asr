﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Word2vec.Tools;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Task_Recognition {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private List<string> topicsList = new List<string>();

        private Task<Vocabulary> vocabLoadingTask;

        public MainPage() {
            InitializeComponent();
            //vocabLoadingTask = Task.Factory.StartNew<Vocabulary>(() => new Word2VecBinaryReader().Read("model.bin"));
        }

        private void setWords_Click(object sender, RoutedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(RecognizerPage), new RecognizerPage.InitParams(vocabLoadingTask, new HashSet<string>(topicsList)));
        }

        private void RichEditBox_TextChanged(System.Object sender, RoutedEventArgs e) {
            var box = sender as RichEditBox;
            Debug.WriteLine(box.Document);
            string text = "";
            box.Document.GetText(Windows.UI.Text.TextGetOptions.UseObjectText, out text);
            topicsList.Clear();

            foreach(string s in text.Split()) {
                topicsList.Add(s.Trim());
            }
        }
    }
}
