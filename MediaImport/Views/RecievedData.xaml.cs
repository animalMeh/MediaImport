﻿using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaImport.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecievedData : Page
    {
        StorageFile[] FilesToImport;
        public RecievedData()
        {
            this.InitializeComponent();
          //  App.RecieveData += ReceiveData;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IReadOnlyList<IStorageItem> files = (IReadOnlyList<IStorageItem>)e.Parameter;
            FilesToImport = GetStorageFiles(files);
            base.OnNavigatedTo(e);
        }

        private StorageFile[] GetStorageFiles(IEnumerable<object> files)
        {
            return (from f in files select (f as StorageFile)).ToArray();
        }


        private async void ImportOneDrive_Click(object sender, RoutedEventArgs e)
        {
            App.AppTile.ChangeTileContent("Import!!!", "Importing to OneDrive");
           
                try
                {
                    var scopes = new string[] { MicrosoftGraphScope.FilesReadWriteAll };
                    OneDriveService.Instance.Initialize("792ff7cc-fc13-4821-8a1e-3ca7f1d590ba", scopes, null, null);
                    if (await OneDriveService.Instance.LoginAsync())
                    {
                        Frame.Navigate(typeof(OneDrivePage), FilesToImport);
                    }
                }
                catch (Exception ex)
                {
                    App.InformMessage = new MessageDialog(ex.Message);
                    await App.InformMessage.ShowAsync();
                }
          
        }

        private async void ImportGoogleDrive_Click(object sender, RoutedEventArgs e)
        {
            App.AppTile.ChangeTileContent("Import!!!", "Importing to Google Drive");

                try
                {
                    if (Models.Google.GoogleDriveService.IsAuthorized)
                        Frame.Navigate(typeof(GoogleDrivePage), FilesToImport);
                    else if (await Models.Google.GoogleDriveService.InitializeInstanceAndLogIn("700806614287-aqq3uguhlld0o7dr0sgs8nd2saci4k7j.apps.googleusercontent.com"))
                        Frame.Navigate(typeof(GoogleDrivePage), FilesToImport);
                }
                catch (Exception ex)
                {
                    App.InformMessage = new MessageDialog(ex.Message);
                    await App.InformMessage.ShowAsync();
                }
           
        }

        private async void ImportGooglePhoto_Click(object sender, RoutedEventArgs e)
        {
            App.AppTile.ChangeTileContent("Import!!!", "Importing to Google Photo");


            if (FilesToImport.Where(f => ((StorageFile)f).ContentType.Contains("audio")).Count() == FilesToImport.Length)
            {
                App.InformMessage = new MessageDialog("Google Photo does not upload audio files");
                await App.InformMessage.ShowAsync();
            }
            else
            {
                try
                {
                    if (Models.Google.GooglePhotoService.IsAuthorized)
                        Frame.Navigate(typeof(GooglePhotoPage), FilesToImport);
                    else if (await Models.Google.GooglePhotoService.InitializeInstanceAndLogIn("700806614287-nkcpk0b2nv4qf2l2aa5aemjks1m02nj9.apps.googleusercontent.com", "I8_WJqquI6aLoB6pl0HuPFE2"))
                        Frame.Navigate(typeof(GooglePhotoPage), FilesToImport);
                }
                catch (Exception ex)
                {
                    App.InformMessage = new MessageDialog(ex.Message);
                    await App.InformMessage.ShowAsync();
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
        }
    }
}