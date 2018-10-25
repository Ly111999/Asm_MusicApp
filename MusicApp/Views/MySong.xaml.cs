using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MusicApp.Entity;
using MusicApp.Services;
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MusicApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MySong : Page
    {
        private bool validName = false;
        private bool validSinger = false;
        private bool validAuthor = false;
        private bool validLink = false;
        private bool validThumbnail = false;

        private ObservableCollection<Song> listSong;
        public static string TokenKey = null;
        private int _currentIndex = 0;
        internal ObservableCollection<Song> ListSong { get => listSong; set => listSong = value; }
        private Song currentSong;
        private bool _isPlaying = false;
        TimeSpan _position;
        DispatcherTimer _timer = new DispatcherTimer();

        public MySong()
        {
            ReadToken();
            this.InitializeComponent();
            this.currentSong = new Song();
            this.ListSong = new ObservableCollection<Song>();
            this.GetSong();
            this.VolumeSlider.Value = 100;
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += ticktock;
            _timer.Start();
        }

        public static async Task<string> ReadToken()
        {
            if (TokenKey == null)
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("token.txt");
                string content = await FileIO.ReadTextAsync(file);
                TokenResponse member_token = JsonConvert.DeserializeObject<TokenResponse>(content);
                Debug.WriteLine("token la: " + member_token.Token);
                TokenKey = member_token.Token;
            }

            return TokenKey;
        }

        public async void GetSong()
        {
            try
            {
                await ReadToken();
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + TokenKey);
                var response = client.GetAsync(APIHandle.GET_SONG);
                var content = await response.Result.Content.ReadAsStringAsync();
                Debug.WriteLine(content);
                ObservableCollection<Song> listSongs = JsonConvert.DeserializeObject<ObservableCollection<Song>>(content);
                foreach (var songs in listSongs)
                {
                    ListSong.Add(songs);
                }
            }
            catch (Exception e)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Please login to listen to music.");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Close") { Id = 1 });
                dialog.CancelCommandIndex = 1;
                await dialog.ShowAsync();
                var rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(Views.Sign_In));
            }
           
        }


        private async void Add_Song(object sender, RoutedEventArgs e)
        {
            //validate client
            ValidateText(Txt_name.Text, Name);
            ValidateText(Txt_thumbnail.Text, Thumbnail);
            ValidateText(Txt_description.Text, Description);
            ValidateText(Txt_singer.Text, Singer);
            ValidateText(Txt_author.Text, Author);
            ValidateLink(Txt_link.Text, Link);
            // get meg error serve
            HttpClient client = new HttpClient();
            this.currentSong.name = this.Txt_name.Text;
            this.currentSong.description = this.Txt_description.Text;
            this.currentSong.singer = this.Txt_singer.Text;
            this.currentSong.author = this.Txt_author.Text;
            this.currentSong.thumbnail = this.Txt_thumbnail.Text;
            this.currentSong.link = this.Txt_link.Text;

            var jsonSong = JsonConvert.SerializeObject(this.currentSong);
            StringContent content = new StringContent(jsonSong, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + TokenKey);
            var response = client.PostAsync(APIHandle.REGISTER_SONG, content);
            var contents = await response.Result.Content.ReadAsStringAsync();
            if (response.Result.StatusCode == HttpStatusCode.Created)
            {
                Debug.WriteLine("Success");
            }
            else
            {
                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(contents);
                if (errorResponse.error.Count > 0)
                {
                    foreach (var key in errorResponse.error.Keys)
                    {
                        var objBykey = this.FindName(key);
                        var value = errorResponse.error[key];
                        if (objBykey != null)
                        {
                            TextBlock textBlock = objBykey as TextBlock;
                            textBlock.Text = "* " + value;
                        }
                    }
                }
            }
            this.Txt_name.Text = String.Empty;
            this.Txt_description.Text = String.Empty;
            this.Txt_singer.Text = String.Empty;
            this.Txt_author.Text = String.Empty;
            this.Txt_thumbnail.Text = String.Empty;
            this.Txt_link.Text = String.Empty;
        }
        
        private void ticktock(object sender, object e)
        {
            if (MediaElement.Position.Seconds < 10)
            {
                MinDuration.Text = MediaElement.Position.Minutes + ":0" + MediaElement.Position.Seconds;
            }
            else
            {
                MinDuration.Text = MediaElement.Position.Minutes + ":" + MediaElement.Position.Seconds;
            }
            if (MediaElement.NaturalDuration.TimeSpan.Seconds < 10)
            {
                MaxDuration.Text = MediaElement.NaturalDuration.TimeSpan.Minutes + ":0" + MediaElement.NaturalDuration.TimeSpan.Seconds;
            }
            else
            {
                MaxDuration.Text = MediaElement.NaturalDuration.TimeSpan.Minutes + ":" + MediaElement.NaturalDuration.TimeSpan.Seconds;
            }
            time_play.Minimum = 0;
            time_play.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            time_play.Value = MediaElement.Position.TotalSeconds;
        }

        private void currentSongs(object sender, TappedRoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            Song chooseSong = panel.Tag as Song;
            LoadSong(chooseSong);
            Debug.WriteLine(chooseSong.link);
            _currentIndex = MyListSong.SelectedIndex;
            Uri mp3Link = new Uri(chooseSong.link);
            this.MediaElement.Source = mp3Link;
            this.Name_song.Text = this.ListSong[_currentIndex].name + " - " + this.ListSong[_currentIndex].singer;
            Do_play();
        }

        private void Do_play()
        {
            _isPlaying = true;
            this.Status_song.Text = "Playing :";
            this.MediaElement.Play();
            PlayButton.Icon = new SymbolIcon(Symbol.Pause);


        }
        private void Do_pause()
        {
            _isPlaying = false;
            this.Status_song.Text = "Pause :";
            this.MediaElement.Pause();
            PlayButton.Icon = new SymbolIcon(Symbol.Play);
        }

        private void Player_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                Do_pause();
            }
            else
            {
                Do_play();
            }
        }

        private void btn_Previous(object sender, RoutedEventArgs e)
        {
            MediaElement.Stop();
            if (_currentIndex >= 0)
            {
                _currentIndex -= 1;
            }
            else
            {
                _currentIndex = listSong.Count - 1;
            }
            Uri mp3Link = new Uri(ListSong[_currentIndex].link);
            this.Name_song.Text = this.ListSong[_currentIndex].name + " - " + this.ListSong[_currentIndex].singer;
            this.MediaElement.Source = mp3Link;
            LoadSong(ListSong[_currentIndex]);
            MyListSong.SelectedIndex = _currentIndex;

            Debug.WriteLine(mp3Link);
            Do_play();

        }

        private void btn_Next(object sender, RoutedEventArgs e)
        {
            MediaElement.Stop();
            if (_currentIndex < ListSong.Count - 1)
            {
                _currentIndex += 1;
            }
            else
            {
                _currentIndex = 0;
            }
            Uri mp3Link = new Uri(ListSong[_currentIndex].link);
            this.Name_song.Text = this.ListSong[_currentIndex].name + " - " + this.ListSong[_currentIndex].singer;
            Debug.WriteLine(mp3Link);
            this.MediaElement.Source = mp3Link;
            LoadSong(ListSong[_currentIndex]);
            Do_play();
            MyListSong.SelectedIndex = _currentIndex;

        }
        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider vol = sender as Slider;
            if (vol != null)
            {
                MediaElement.Volume = vol.Value / 100;
                this.volume.Text = vol.Value.ToString();
            }
        }

        private void LoadSong(Entity.Song currentSong)
        {
            this.Status_song.Text = "Loading";
            MediaElement.Source = new Uri(currentSong.link);
            Debug.WriteLine(MediaElement.NaturalDuration.TimeSpan.TotalSeconds);
            this.Status_song.Text = currentSong.name + " - " + currentSong.singer;
        }

        private void Songs(Song songs)
        {
            try
            {
                this.Name_song.Text = songs.name;
                this.Singer.Text = songs.singer;
                this.MediaElement.Source = new Uri(songs.link);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private void AutoNext(ObservableCollection<Song> songs, ListView list)
        {
            if (_currentIndex < ListSong.Count - 1 && _currentIndex >= 0)
            {
                _currentIndex++;
                Songs(ListSong[_currentIndex]);
                Do_play();
                MyListSong.SelectedIndex = _currentIndex;
            }
            else
            {
                _currentIndex = 0;
                Songs(ListSong[_currentIndex]);
                Do_play();
                MyListSong.SelectedIndex = _currentIndex;

            }

        }

        private void Check_song_ended(object sender, RoutedEventArgs e)
        {
            if (MyListSong.SelectedIndex == (ListSong.Count - 1))
            {
                Debug.WriteLine(1);
                AutoNext(ListSong, MyListSong);
                PlayButton.Icon = new SymbolIcon(Symbol.Pause);
                _isPlaying = true;
            }
            else
            {
                Debug.WriteLine(2);
                AutoNext(ListSong, MyListSong);
            }
            
        }

        private void time_play_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double SliderValue = time_play.Value;
            TimeSpan ts = TimeSpan.FromSeconds(SliderValue);
            MediaElement.Position = ts;
        }

        public static bool ValidateText(string input, TextBlock textBlock)
        {
            if (input.Length > 0)
            {
                textBlock.Text = "";
                return true;
            }
            else
            {
                textBlock.Text = "*Not empty or null";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);
                return false;
            }
        }

        public static bool ValidateLink(string input, TextBlock textBlock)
        {
            Regex regex = new Regex(@"((https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))+[\w\d:#@%/;$()~_?\+-=\\\.&]*\.mp3)");

            if (string.IsNullOrWhiteSpace(input))
            {
                textBlock.Text = "*Link song can't be empty or null";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);

                return false;
            }
            else if (!regex.IsMatch(input))
            {
                textBlock.Text = "*Invalid: example.mp3";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);

                return false;
            }
            else
            {
                textBlock.Text = "";
                return true;
            }
        }
    }
    
}
