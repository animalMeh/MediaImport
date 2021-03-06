﻿using MediaImport.Models;
using MediaImport.Models.Notifications;
using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MediaImport.Views
{
    public sealed partial class RecievedData : Page
    {
        StorageFile[] FilesToImport;

        public RecievedData()
        {
            this.InitializeComponent();
        }

        protected override  async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<StorageFile> ImportedFiles = new List<StorageFile>();
            IReadOnlyList<IStorageItem> files = (IReadOnlyList<IStorageItem>)e.Parameter;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            foreach (var f in files)
            {
                StorageFile newFile = await (f as StorageFile).CopyAsync(localFolder ,f.Name , NameCollisionOption.ReplaceExisting);
                ImportedFiles.Add(newFile);
            }
            FilesToImport = ImportedFiles.ToArray();
            base.OnNavigatedTo(e);
        }

        private StorageFile[] GetStorageFiles(IEnumerable<object> files)
        {
            return (from f in files select (f as StorageFile)).ToArray();
        }


        private async void ImportOneDrive_Click(object sender, RoutedEventArgs e)
        {
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
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
            App.AppTile.ChangeTileContent(TileType.ImageTile, string.Format("Import {0} file(s)", FilesToImport.Length), imageSource: "ms-appx:///Assets/MyLogo/onedrive.png");

        }

        private async void ImportGoogleDrive_Click(object sender, RoutedEventArgs e)
        {

            try
                {
                    if (Models.Google.GoogleDriveService.IsAuthorized)
                        Frame.Navigate(typeof(GoogleDrivePage), FilesToImport);
                    else if (await Models.Google.GoogleDriveService.InitializeInstanceAndLogIn("700806614287-aqq3uguhlld0o7dr0sgs8nd2saci4k7j.apps.googleusercontent.com"))
                        Frame.Navigate(typeof(GoogleDrivePage), FilesToImport);
                }
            catch (Exception ex)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }

            App.AppTile.ChangeTileContent(TileType.ImageTile, string.Format("Import {0} file(s)", FilesToImport.Length), imageSource: "ms-appx:///Assets/MyLogo/googledrive.png");

        }

        private async void ImportGooglePhoto_Click(object sender, RoutedEventArgs e)
        {
         
            if (FilesToImport.Where(f => ((StorageFile)f).ContentType.Contains("audio")).Count() == FilesToImport.Length)
            {
                NotificateMessageDialog.InformMessage = new MessageDialog("Google Photo does not upload audio files");
                await NotificateMessageDialog.InformMessage.ShowAsync();
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
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
                App.AppTile.ChangeTileContent(TileType.ImageTile, string.Format("Import {0} file(s)", FilesToImport.Length), imageSource: "ms-appx:///Assets/MyLogo/googlephoto.png");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
        }
    }
}
