using System;
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
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using MediaImport.Models.Notifications;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace MediaImport.Views
{
    public sealed partial class RootPage : Page
    {
        BitmapImage note = new BitmapImage(new Uri("ms-appx:///Assets/MyLogo/note.png"));
        BitmapImage video = new BitmapImage(new Uri("ms-appx:///Assets/MyLogo/video.png"));

        DeviceWatcher UsbWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
        bool IsUserUsesDrives = false;
        bool IsUserInjectDrive = false;
        DriveViewModel ConnectedDrives;

        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();


        public RootPage()
        {
            this.InitializeComponent();
     
            ConnectedDrives = new DriveViewModel();
            DataContext = ConnectedDrives;
           // IconsListView.ItemsSource = ConnectedDrives;

            BackButton.Visibility = Visibility.Collapsed;
            UsbWatcher.Added += Insertion;
            UsbWatcher.Removed += Extraction;
            ShowControllers();
            UsbWatcher.Start();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is IReadOnlyList<StorageFile>)
            {
                FolderFiles.ItemsSource = e.Parameter as IReadOnlyList<StorageFile>;
            }
            App.AppTile.ChangeTileContent(TileType.TextTile, "Import", "Can share your files to 3 different clouds");
            base.OnNavigatedTo(e);
        }

        private void HumbButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GC.WaitForPendingFinalizers();
            GC.Collect(0);
            DriveFolders.ItemsSource = (IconsListView.SelectedItem as Drive).Folders;
            FolderFiles.ItemsSource = (IconsListView.SelectedItem as Drive).Files;
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
        //needs to check
        private void IconsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsUserInjectDrive && ConnectedDrives.PortableDrives.Count != 0)
            {
                IsUserUsesDrives = true;
                ShowControllers();
                
                DriveFolders.ItemsSource = (IconsListView.SelectedItem as Drive).Folders;
                FolderFiles.ItemsSource = (IconsListView.SelectedItem as Drive).Files;
            }
            else IsUserInjectDrive = false;
        }

        public async void DriveFolders_ItemClick(object sender, ItemClickEventArgs e)
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
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
            BitmapImage img = null;
            switch (fileType)
            {
                case ".mp3":
                case ".mpeg":
                    img =  note;
                    break;
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
                                img =  photo;
                        }
                    break;
                case ".webm":
                case ".mp4":
                case ".3gp":
                    img =  video;
                    break;
            }
            return img;
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
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
            }
            else
            {
                MessageDialog InformMessage = new MessageDialog("Select at least 1 file to upload");
                await InformMessage.ShowAsync();
            }
           App.AppTile.ChangeTileContent(TileType.ImageTile , string.Format("Import {0} file(s)", FolderFiles.SelectedItems.Count), imageSource: "ms-appx:///Assets/MyLogo/onedrive.png");

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
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
            }
            else
            {
                NotificateMessageDialog.InformMessage = new MessageDialog("Select at least 1 file to upload");
                await NotificateMessageDialog.InformMessage.ShowAsync();
            }

            App.AppTile.ChangeTileContent(TileType.ImageTile, string.Format("Import {0} file(s)", FolderFiles.SelectedItems.Count), imageSource: "ms-appx:///Assets/MyLogo/googledrive.png");

        }

        private async void ImportGooglePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (CanImport())
            {
                var files = FolderFiles.SelectedItems.ToArray();
                if (files.Where(f => ((StorageFile)f).ContentType.Contains("audio")).Count() == files.Length)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog("Google Photo does not upload audio files");
                    await NotificateMessageDialog.InformMessage.ShowAsync();
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
                        NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                        await NotificateMessageDialog.InformMessage.ShowAsync();
                    }
                }
            }
            else
            {
                NotificateMessageDialog.InformMessage = new MessageDialog("Select at least 1 file to upload");
                await NotificateMessageDialog.InformMessage.ShowAsync();
            }

            App.AppTile.ChangeTileContent(TileType.ImageTile, string.Format("Import {0} file(s)", FolderFiles.SelectedItems.Count), imageSource: "ms-appx:///Assets/MyLogo/googlephoto.png");

        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            if (CanImport())
            {
                DataTransferManager.ShowShareUI();
            }
        }

        private void DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = "Share Example";
            request.Data.Properties.Description = "A demonstration on how to share";
            var files = GetStorageFiles(FolderFiles.SelectedItems);
            request.Data.SetStorageItems(files);  
        }
        
        private IEnumerable<StorageFile> GetStorageFiles(IEnumerable<object> files)
        {
            return from f in files select (f as StorageFile);
        }
            
    }
}
