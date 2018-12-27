﻿using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MediaImport.ViewModels;
using MediaImport.Models;
using System.IO;
using System.Linq;
using Windows.Storage;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using Windows.UI.Popups;

namespace MediaImport.Views
{
    public sealed partial class RootPage : Page
    {
        BitmapImage note = new BitmapImage(new Uri("ms-appx:///Assets/MyLogo/note.png"));
        BitmapImage video = new BitmapImage(new Uri("ms-appx:///Assets/MyLogo/video.png"));

        DeviceWatcher UsbWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
        bool IsUserUsesDrives = false;
        bool IsUserInjectDrive = false;
        PortableDriveViewModel ConnectedDrives;

        public RootPage()
        {
            this.InitializeComponent();
            ConnectedDrives = new PortableDriveViewModel();
            DataContext = ConnectedDrives;
            BackButton.Visibility = Visibility.Collapsed;
            UsbWatcher.Added += Insertion;
            UsbWatcher.Removed += Extraction;
            ShowControllers();
            UsbWatcher.Start();
        }

        private void HumbButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DriveFolders.ItemsSource = (IconsListView.SelectedItem as PortableDrive).Folders;
            FolderFiles.ItemsSource = (IconsListView.SelectedItem as PortableDrive).Files;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            IsUserUsesDrives = false;
            ShowControllers();
        }

        private void IconsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            IsUserUsesDrives = true;
            ShowControllers();
        }

        private void IconsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsUserInjectDrive)
            {
                IsUserUsesDrives = true;
                ShowControllers();
                DriveFolders.ItemsSource = (IconsListView.SelectedItem as PortableDrive).Folders;
                FolderFiles.ItemsSource = (IconsListView.SelectedItem as PortableDrive).Files;
            }
            else IsUserInjectDrive = false;
        }

        public async void DriveFolders_ItemClick(object sender, ItemClickEventArgs e)
        {
            BackButton.Visibility = Visibility.Visible;
            IReadOnlyList<StorageFolder> Folders = await (e.ClickedItem as StorageFolder).GetFoldersAsync();
            DriveFolders.ItemsSource = Folders;
            IReadOnlyList<StorageFile> Files = await (e.ClickedItem as StorageFolder).GetFilesAsync();
            FolderFiles.ItemsSource = Files;
        }

        private async void Image_Loaded(object sender, RoutedEventArgs e)
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Windows.UI.Xaml.Controls.Image img = (sender as Windows.UI.Xaml.Controls.Image);
            var localPath = ((BitmapImage)img.Source).UriSource.LocalPath;
            try
            {

                img.Source = await GetIconOfFile(localPath);
            }
            catch (Exception ex)
            {
                if (ex.Message == "The component cannot be found")
                    return;
            }
            var file = await StorageFile.GetFileFromPathAsync(localPath);
        }

        private async Task<BitmapImage> GetIconOfFile(string localPath)
        {
            string fileType = Path.GetExtension(localPath);
            switch (fileType)
            {
                case ".mp3":
                case ".mpeg":
                    return note;
                case ".jpg":
                case ".JPG":
                case ".png":
                case ".PNG":
                case ".jpeg":
                        var file = await StorageFile.GetFileFromPathAsync(localPath);
                        using (var stream = await file.OpenAsync(FileAccessMode.Read))
                        {
                        BitmapImage photo = new BitmapImage();
                        photo.DecodePixelWidth = 100;
                        await photo.SetSourceAsync(stream);
                                return photo;
                        }
                case ".webm":
                case ".mp4":
                case ".3gp":
                    return video;
                default:
                    return null;
            }
        }
        
        private async void Extraction(DeviceWatcher sender, DeviceInformationUpdate e)
        {
            await this.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () =>
            {
                ConnectedDrives.Remove();
                HomeButton_Click(new object(), new RoutedEventArgs());
                IsUserInjectDrive = true;
                IsUserUsesDrives = false;
                ShowControllers();
            });
        }

        private async void Insertion(DeviceWatcher sender, DeviceInformation e)
        {
            await this.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () =>
            {
                ConnectedDrives.AddNewOne();
            });
        }

        private void ShowControllers()
        {
            if (IsUserUsesDrives)
            {
                EmptyNovel.Visibility = Visibility.Collapsed;
                DriveFolders.Visibility = Visibility.Visible;
                FolderFiles.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyNovel.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Collapsed;
                DriveFolders.Visibility = Visibility.Collapsed;
                FolderFiles.Visibility = Visibility.Collapsed;
            }
        }

        private bool CanImport()
        {
            return FolderFiles.SelectedItems.Count > 0;
        }

        public void FilesUploading(bool u)
        {
            OnProgressPanel.Visibility = u ? Visibility.Visible : Visibility.Collapsed;
        }
   
        private async void ImportOneDrive_Click(object sender, RoutedEventArgs e)
        {
            if (CanImport())
            {
                try
                {
                    var scopes = new string[] { MicrosoftGraphScope.FilesReadWriteAll };
                    OneDriveService.Instance.Initialize("792ff7cc-fc13-4821-8a1e-3ca7f1d590ba", scopes, null, null);
                    if (await OneDriveService.Instance.LoginAsync())
                    {
                        Frame.Navigate(typeof(OneDrivePage), FolderFiles.SelectedItems.ToArray());
                    }
                }
                catch(Exception ex)
                {
                    App.InformMessage = new MessageDialog(ex.Message);
                    await App.InformMessage.ShowAsync();
                }
            }
            else
            {
                App.InformMessage = new MessageDialog("Select at least 1 file to upload");
                await App.InformMessage.ShowAsync();
            }
        }

        private async void ImportGoogleDrive_Click(object sender, RoutedEventArgs e)
        {
            if (CanImport())
            {
                try
                {
                    if (Models.Google.GoogleDriveService.IsAuthorized)
                        Frame.Navigate(typeof(GoogleDrivePage), FolderFiles.SelectedItems.ToArray());
                    else if (await Models.Google.GoogleDriveService.InitializeInstanceAndLogIn("700806614287-aqq3uguhlld0o7dr0sgs8nd2saci4k7j.apps.googleusercontent.com"))
                        Frame.Navigate(typeof(GoogleDrivePage), FolderFiles.SelectedItems.ToArray());
                }
                catch (Exception ex)
                {
                    App.InformMessage = new MessageDialog(ex.Message);
                    await App.InformMessage.ShowAsync();
                }
            }
            else
            {
                App.InformMessage = new MessageDialog("Select at least 1 file to upload");
                await App.InformMessage.ShowAsync();
            }
        }

        private async void ImportGooglePhoto_Click(object sender, RoutedEventArgs e)
        {

            if (CanImport())
            {
                var files = FolderFiles.SelectedItems.ToArray();
                if (files.Where(f => ((StorageFile)f).ContentType.Contains("audio")).Count() == files.Length)
                {
                    App.InformMessage = new MessageDialog("Google Photo does not upload audio files");
                    await App.InformMessage.ShowAsync();
                }
                else
                {
                    try
                    {
                        if (Models.Google.GooglePhotoService.IsAuthorized)
                            Frame.Navigate(typeof(GooglePhotoPage), FolderFiles.SelectedItems.ToArray());
                        else if (await Models.Google.GooglePhotoService.InitializeInstanceAndLogIn("700806614287-nkcpk0b2nv4qf2l2aa5aemjks1m02nj9.apps.googleusercontent.com", "I8_WJqquI6aLoB6pl0HuPFE2"))
                            Frame.Navigate(typeof(GooglePhotoPage), FolderFiles.SelectedItems.ToArray());
                    }
                    catch (Exception ex)
                    {
                        App.InformMessage = new MessageDialog(ex.Message);
                        await App.InformMessage.ShowAsync();
                    }
                }
            }
            else
            {
                App.InformMessage = new MessageDialog("Select at least 1 file to upload");
                await App.InformMessage.ShowAsync();
            }
        }
    }
}