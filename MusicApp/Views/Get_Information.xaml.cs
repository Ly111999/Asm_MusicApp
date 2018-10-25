using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
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
    public sealed partial class Get_Information : Page
    {
        public Get_Information()
        {
            this.InitializeComponent();
            GetUser();
        }

        public async void GetUser()
        {
            try
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("token.txt");
                string contents = await FileIO.ReadTextAsync(file);
                TokenResponse member_token = JsonConvert.DeserializeObject<TokenResponse>(contents);

                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + member_token.Token);
                var resp = httpClient.GetAsync(APIHandle.API_INFORMATION).Result;
                var content = await resp.Content.ReadAsStringAsync();
                Member member = JsonConvert.DeserializeObject<Member>(content);
                Debug.WriteLine(content);


                txt_name.Text = member.firstName + member.lastName;
                string[] date = member.birthday.Split('T');
               
                avatar.Source = new BitmapImage(new Uri(member.avatar, UriKind.Absolute));
                txt_email.Text = member.email;
                txt_phone.Text = member.phone;
                txt_introduction.Text = member.introduction;
                txt_address.Text = member.address;
                txt_birthday.Text = date[0];
                Debug.WriteLine(member);
            }
            catch (Exception e)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Please login to view the account information.");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Close") { Id = 1 });
                dialog.CancelCommandIndex = 1;
                await dialog.ShowAsync();
                var rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(Sign_In));
            }
            

        }
    }
}
