using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    public sealed partial class Sign_In : Page
    {
        private static Member currentLogin;
        public Sign_In()
        {
            this.InitializeComponent();
        }

        
        private async void Button_submit(object sender, RoutedEventArgs e)
        {
            var checkEmail = ValidateEmail(this.Email.Text, email);
            ValidatePassword(Password.Password, password);
            if (checkEmail)
            {
                
                Dictionary<string, string> login_handle = new Dictionary<string, string>();
                login_handle.Add("email", this.Email.Text);
                login_handle.Add("password", this.Password.Password);

                HttpClient httpClient = new HttpClient();
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(login_handle), System.Text.Encoding.UTF8, "application/json");
                var response = httpClient.PostAsync(APIHandle.API_LOGIN, stringContent).Result;
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Debug.WriteLine("Login Success");
                    Debug.WriteLine(responseContent);
                    TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent); //read token
                    StorageFolder folder = ApplicationData.Current.LocalFolder;// save token file
                    StorageFile storageFile = await folder.CreateFileAsync("token.txt", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(storageFile, responseContent);

                    var rootFrame = Window.Current.Content as Frame;
                    rootFrame.Navigate(typeof(MainPage));
                }
                else
                {
                    ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                    if (errorResponse.error.Count > 0)
                    {
                        foreach (var key in errorResponse.error.Keys)
                        {
                            var objectBykey = this.FindName(key);
                            var value = errorResponse.error[key];
                            if (objectBykey != null)
                            {
                                TextBlock textBlock = objectBykey as TextBlock;
                                textBlock.Text = "* " + value;
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
            
        }

        public static async void DoLogin()
        {
            // Auto login nếu tồn tại file token 
            currentLogin = new Member();
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync("token.txt") != null)
            {
                StorageFile file = await folder.GetFileAsync("token.txt");
                var tokenContent = await FileIO.ReadTextAsync(file);

                TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(tokenContent);

                // Lay thong tin ca nhan bang token.
                HttpClient client2 = new HttpClient();
                client2.DefaultRequestHeaders.Add("Authorization", "Basic " + token.Token);
                var resp = client2.GetAsync(APIHandle.API_INFORMATION).Result;
                Debug.WriteLine(await resp.Content.ReadAsStringAsync());
                var userInfoContent = await resp.Content.ReadAsStringAsync();

                Member userInfoJson = JsonConvert.DeserializeObject<Member>(userInfoContent);

                currentLogin.firstName = userInfoJson.firstName;
                currentLogin.lastName = userInfoJson.lastName;
                currentLogin.avatar = userInfoJson.avatar;
                currentLogin.phone = userInfoJson.phone;
                currentLogin.address = userInfoJson.address;
                currentLogin.introduction = userInfoJson.introduction;
                currentLogin.gender = userInfoJson.gender;
                currentLogin.birthday = userInfoJson.birthday;
                currentLogin.email = userInfoJson.email;
                currentLogin.password = userInfoJson.password;
                currentLogin.status = userInfoJson.status;
                var rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));


                Debug.WriteLine("Success");
            }
            else
            {
                Debug.WriteLine("File doesn't exist");
            }
        }

        private void Sign_up(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(Views.Sign_Up));
        }

        private async void Auto_Login(object sender, RoutedEventArgs e)
        {
            DoLogin();
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

    }
}
