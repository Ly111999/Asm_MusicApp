using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
    public sealed partial class Sign_Up : Page
    {
        private string currentUploadUrl;
        private Member currentMember;

        public Sign_Up()
        {
            this.currentMember = new Member();
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private StorageFile photo;
        private async void Choose_Image(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            this.photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (this.photo == null)
            {
                // User cancelled photo capture
                return;
            }
            HttpClient httpClient = new HttpClient();
            currentUploadUrl = await httpClient.GetStringAsync(APIHandle.GET_UPLOAD_URL);
            Debug.WriteLine("Upload url: " + currentUploadUrl);
            HttpUploadFile(currentUploadUrl, "myFile", "image/png");
        }

        public async void HttpUploadFile(string url, string paramName, string contentType)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";

            Stream rs = await wr.GetRequestStreamAsync();
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", paramName, "path_file", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            // write file.
            Stream fileStream = await this.photo.OpenStreamForReadAsync();
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            WebResponse wresp = null;
            try
            {
                wresp = await wr.GetResponseAsync();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string imageUrl = reader2.ReadToEnd();
                Avatar.Source = new BitmapImage(new Uri(imageUrl, UriKind.Absolute));
                AvatarUrl.Text = imageUrl;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error uploading file", ex.StackTrace);
                Debug.WriteLine("Error uploading file", ex.InnerException);
                if (wresp != null)
                {
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        private async void Submit(object sender, RoutedEventArgs e)
        {
            //validate client
            var checkEmail = ValidateEmail(this.Email.Text, email);
            ValidatePassword(Password.Password, password);
            ValidateText(FirstName.Text, firstName);
            ValidateText(LastName.Text, lastName);
            ValidateText(AvatarUrl.Text, avatar);
            ValidatePhone(Phone.Text, phone);
            ValidateText(Address.Text, address);
            if (checkEmail)
            {
                // validate data.
                this.currentMember.firstName = this.FirstName.Text;
                this.currentMember.lastName = this.LastName.Text;
                this.currentMember.avatar = this.AvatarUrl.Text;
                this.currentMember.address = this.Address.Text;
                this.currentMember.introduction = this.Introduction.Text;
                this.currentMember.phone = this.Phone.Text;
                this.currentMember.email = this.Email.Text;
                this.currentMember.password = this.Password.Password;

                string jsonMember = JsonConvert.SerializeObject(this.currentMember);

                HttpClient httpClient = new HttpClient();
                var content = new StringContent(jsonMember, Encoding.UTF8, "application/json");
                var response = httpClient.PostAsync(APIHandle.MEMBER_REGISTER, content);
                var contents = await response.Result.Content.ReadAsStringAsync();
                if (response.Result.StatusCode == HttpStatusCode.Created)
                {
                    var rootFrame = Window.Current.Content as Frame;
                    rootFrame.Navigate(typeof(Views.Sign_In));
                }
                else
                {
                    ErrorResponse errorsResponse = JsonConvert.DeserializeObject<ErrorResponse>(contents);
                    if (errorsResponse.error.Count > 0)
                    {
                        foreach (var key in errorsResponse.error.Keys)
                        {
                            var objectBykey = this.FindName(key);
                            var value = errorsResponse.error[key];
                            if (objectBykey != null)
                            {
                                TextBlock textBlock = objectBykey as TextBlock;
                                textBlock.Text = "* " + value;

                            }
                        }
                    }
                }
            }
            //// validate data.
            //this.currentMember.firstName = this.FirstName.Text;
            //this.currentMember.lastName = this.LastName.Text;
            //this.currentMember.avatar = this.AvatarUrl.Text;
            //this.currentMember.address = this.Address.Text;
            //this.currentMember.introduction = this.Introduction.Text;
            //this.currentMember.phone = this.Phone.Text;
            //this.currentMember.email = this.Email.Text;
            //this.currentMember.password = this.Password.Password;

            //string jsonMember = JsonConvert.SerializeObject(this.currentMember);

            //HttpClient httpClient = new HttpClient();
            //var content = new StringContent(jsonMember, Encoding.UTF8, "application/json");
            //var response = httpClient.PostAsync(APIHandle.MEMBER_REGISTER, content);
            //var contents = await response.Result.Content.ReadAsStringAsync();
            //if (response.Result.StatusCode == HttpStatusCode.Created)
            //{
            //    var rootFrame = Window.Current.Content as Frame;
            //    rootFrame.Navigate(typeof(Views.Sign_In));
            //}
            //else
            //{
            //    ErrorResponse errorsResponse = JsonConvert.DeserializeObject<ErrorResponse>(contents);
            //    if (errorsResponse.error.Count > 0)
            //    {
            //        foreach (var key in errorsResponse.error.Keys)
            //        {
            //            var objectBykey = this.FindName(key);
            //            var value = errorsResponse.error[key];
            //            if (objectBykey != null)
            //            {
            //                TextBlock textBlock = objectBykey as TextBlock;
            //                textBlock.Text = "* " + value;

            //            }
            //        }
            //    }
            //}
         
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            this.currentMember.gender = Int32.Parse(radio.Tag.ToString());
        }

        private void BirthdayPicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            this.currentMember.birthday = sender.Date.Value.ToString("yyyy-MM-dd");
        }

        private void Sign_in(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(Views.Sign_In));

        }

        private async void Upload_Image(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            this.photo = await openPicker.PickSingleFileAsync();
            HttpClient httpClient = new HttpClient();
            currentUploadUrl = await httpClient.GetStringAsync(Services.APIHandle.GET_UPLOAD_URL);
            Debug.WriteLine("Upload url: " + currentUploadUrl);
            HttpUploadFile(currentUploadUrl, "myFile", "image/png");

        }

        public static bool ValidateEmail(string input, TextBlock textBlock)
        {
            Regex regex = new Regex(@"^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
                                    + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$");
            if (string.IsNullOrWhiteSpace(input))
            {
                textBlock.Text = "*Email can't be empty or null";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);

                return false;
            }
            else if (!regex.IsMatch(input))
            {
                textBlock.Text = "*Invalid: example@gmail.com";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);

                return false;
            }
            else
            {
                textBlock.Text = "";
                return true;
            }

        }

        public static bool ValidatePassword(string input, TextBlock textBlock)
        {
            if (input.Length > 0)
            {
                if (input.Length < 6 || input.Length > 24)
                {
                    textBlock.Text = "*Must be from 6 to 24 characters";
                    textBlock.Foreground = new SolidColorBrush(Colors.Red);
                    return false;
                }
                else
                {
                    textBlock.Text = "";
                    return true;
                }
            }
            else
            {
                textBlock.Text = "* Password can't be empty or null";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);
                return false;
            }
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

        public static bool ValidatePhone(string input, TextBlock textBlock)
        {
            Regex regex = new Regex(@"^\s*\+?\s*([0-9][\s-]*){10,13}$");
            if (input.Length > 0)
            {
                if (!regex.IsMatch(input))
                {
                    textBlock.Text = "*Must be 10 or 11 numbers";
                    textBlock.Foreground = new SolidColorBrush(Colors.Red);
                    return false;
                }
                else
                {
                    textBlock.Text = "";
                    return true;
                }
            }
            else
            {
                textBlock.Text = "*Not empty or null";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);
                return false;
            }
        }
    }

}
