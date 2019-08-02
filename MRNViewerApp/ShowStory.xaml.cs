using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using MRNWebsocketViewer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRNWebsocketViewer
{
    /// <summary>
    /// Interaction logic for ShowStory.xaml
    /// </summary>
    public partial class ShowStory : Window
    {
        public ShowStory()
        {
            InitializeComponent();
    
        }

        public MrnStory MrnStoryData { get; set; }

        private void SaveJsonBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.Filter = "Json files (*.json)|*.json";
                saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
                if (saveFileDialog.ShowDialog() != true) return;
                using (var fs = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate))
                using (var sw = new StreamWriter(fs))
                using (var jsw = new JsonTextWriter(sw))
                {
                    jsw.Formatting = Formatting.Indented;

                    var serializer = new JsonSerializer();
                    serializer.Serialize(jsw, JToken.Parse(MrnStoryData.Story.ToJson()));
                }

                MessageBox.Show($"Save json data to {saveFileDialog.FileName} Completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Writing Json to file Error\r\n {ex.Message}");

            }
        }

        private void BodyToClipBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject((object)MrnStoryData.Story.Body);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show("Copied Body to Clipboard");
        }
    }
}
