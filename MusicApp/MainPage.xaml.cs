using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MusicApp.Entity;
using MusicApp.Views;
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MusicApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static StorageFolder folder = ApplicationData.Current.LocalFolder;                                       
        private string CurrentTag = " ";
        public MainPage()
        {
            this.InitializeComponent();
            this.My_Frame.Navigate(typeof(Views.Home));
        }


        public async void Logout()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync("token.txt") != null)
            {
                StorageFile file = await folder.GetFileAsync("token.txt");
                await file.DeleteAsync();
                var dialog = new Windows.UI.Popups.MessageDialog("Logout success.See you again!!!");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Close") { Id = 1 });
                dialog.CancelCommandIndex = 1;
                Debug.WriteLine(" logouted !!!");
                await dialog.ShowAsync();
            }
        }

        private void btn_bar_Click(object sender, RoutedEventArgs e)
        {
            this.My_SplitView.IsPaneOpen = !this.My_SplitView.IsPaneOpen;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (CurrentTag == radio.Tag.ToString())
            {
                return;
            }
            switch (radio.Tag.ToString())
            {
                case "Home":
                    CurrentTag = "Home";
                    this.My_Frame.Navigate(typeof(Views.Home));
                    break;
                case "MySong":
                    CurrentTag = "MySong";
                    this.My_Frame.Navigate(typeof(Views.MySong));
                    break;
                case "My_account":
                    CurrentTag = "My_account";
                    this.My_Frame.Navigate(typeof(Views.Get_Information));
                    break;
                case "Logout":
                    CurrentTag = "Logout";
                    Logout();
                    var frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(Views.Sign_In));
                    break;
            }
        }

        
    }

}
