using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using youtuber.net;
using youtuber.Net.Youtube;

namespace youtuberExample {
    public partial class MainWindow : Window
    {
        private Video currentVideo;
        private List<VideoFile> downloadFiles;

        public MainWindow() {
            InitializeComponent();
        }

        private void ExecuteBtn_OnClick(object sender, RoutedEventArgs e){
            if (Uri.IsWellFormedUriString(InputTB.Text, UriKind.Absolute)) {
                Uri uri = new Uri(InputTB.Text);
                URLResult result = URLUtility.AnalyzeURI(uri);
                if (!result.HasFlag(URLResult.IsValid)) {return;}
                if (result.HasFlag(URLResult.IsVideo)) {
                    LoadVideo(URLUtility.ExtractVideoID(uri));
                }
            }
        }

        private async void LoadVideo(string videoId){
            currentVideo = await Video.fromID(videoId);
            downloadFiles = currentVideo.ExtractFiles();
            foreach (VideoFile downloadFile in downloadFiles) {
                if (downloadFile.GetType() == typeof(VideoFile.Normal)) {
                    NormalVideoLB.Items.Add(downloadFile);
                } else if (downloadFile.GetType() == typeof(VideoFile.DashVideo)) {
                    DashVideoLB.Items.Add(downloadFile);
                } else if (downloadFile.GetType() == typeof(VideoFile.DashAudio)) {
                    DashAudioLB.Items.Add(downloadFile);
                }
            }
        }
    }
}
