using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using youtuber.Net;
using youtuber.Net.Youtube;
using youtuber.Net.Youtube.Official;
using Image = youtuber.Net.Youtube.Image;

namespace youtuberExample
{
    public partial class MainWindow : Window
    {
        private string apiKey;
        private Video currentVideo;

        private DirectoryInfo downloadDir = new DirectoryInfo(Path.GetDirectoryName(Application.ResourceAssembly.Location));
        private string filename;

        private List<VideoFile> downloadFiles;
        private Search searcher;

        private List<Search.Result> SearchResults;

        public MainWindow(){
            InitializeComponent();
            LocationTB.Text = downloadDir.FullName;
        }

        private void ExecuteBtn_OnClick(object sender, RoutedEventArgs e){
            if (Uri.IsWellFormedUriString(InputTB.Text, UriKind.Absolute)) {
                Uri uri = new Uri(InputTB.Text);
                URLResult result = URLUtility.AnalyzeURI(uri);
                if (!result.HasFlag(URLResult.IsValid)) return;
                if (result.HasFlag(URLResult.IsVideo)) LoadVideo(URLUtility.ExtractVideoID(uri));
            } else { SearchString(InputTB.Text); }
        }

        private async void LoadVideo(string videoId){
            VideoTab.IsSelected = true;
            currentVideo = await Video.fromID(videoId);

            ImagePreview.Source = new BitmapImage(Image.FromID(videoId, ImageType.MaximumResolutionDefault));
            filename = currentVideo.Title;
            LocationTB.Text = Path.Combine(downloadDir.FullName, filename);

            downloadFiles = currentVideo.ExtractFiles();
            FormatLB.Items.Clear();
            downloadFiles.ForEach(f => FormatLB.Items.Add(f));
        }

        private async void SearchString(string query){
            SearchTab.IsSelected = true;
            if (string.IsNullOrEmpty(apiKey)) apiKey = await LoadApiKey();
            searcher = new Search(apiKey);

            string[] options = {Search.Params.MaxResults(20)};
            await searcher.Execute(query, options);
            if (!searcher.Success) return;
            SearchListView.Items.Clear();

            SearchResults = searcher.Results;
            foreach (Search.Result result in SearchResults) {
                ListViewItem lvi = new ListViewItem();
                lvi.Content = result.Title;
                SearchListView.Items.Add(lvi);
            }
        }

        private static async Task<string> LoadApiKey(){
            Video video = await Video.fromID("xkUk57wuKKM");
            BaseDotJs baseDotJs = await BaseDotJs.GetBaseDotJs(video.PlayerVersion, video.Cookies);
            return baseDotJs.ExtractApiKey();
        }

        private async void DownloadBtn_OnClick(object sender, RoutedEventArgs e){
            int index = FormatLB.SelectedIndex;
            VideoFile videoFile = downloadFiles[index];
            Uri downloadUri = await videoFile.GetDownloadUri();
            using (WebClient webClient = new WebClient()) {
                await webClient.DownloadFileTaskAsync(downloadUri, Path.Combine(downloadDir.FullName, filename));
            }
        }

        private void SearchListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e){
            int index = SearchListView.SelectedIndex;
            if (index < 0) return;
            if (SearchResults[index].GetType() != typeof(Search.Result.Video)) return;
            Search.Result.Video videoResult = (Search.Result.Video) SearchResults[index];
            LoadVideo(videoResult.VideoID);
        }

        private void LocationBtn_OnClick(object sender, RoutedEventArgs e){
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = downloadDir.FullName;
            
            if ((bool) !sfd.ShowDialog(Owner)) return;
            downloadDir = new DirectoryInfo(Path.GetDirectoryName(sfd.FileName));
            filename = Path.GetFileName(sfd.FileName);
            LocationTB.Text = Path.Combine(downloadDir.FullName, filename);
        }

        private void FormatLB_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            DownloadBtn.IsEnabled = true;
            downloadDir = new DirectoryInfo(Path.GetDirectoryName(LocationTB.Text));
            filename = Path.GetFileNameWithoutExtension(LocationTB.Text);
            int index = FormatLB.SelectedIndex;
            VideoFile videoFile = downloadFiles[index];
            filename += videoFile.Extension;
            LocationTB.Text = Path.Combine(downloadDir.FullName, filename);
        }

        private void LocationTB_OnTextChanged(object sender, TextChangedEventArgs e) {
            downloadDir = new DirectoryInfo(Path.GetDirectoryName(LocationTB.Text));
            filename = Path.GetFileNameWithoutExtension(LocationTB.Text);
        }
    }
}