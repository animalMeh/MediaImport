using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MediaImport.Models.Google;
using System.Threading.Tasks;
using Windows.UI.Popups;
using MediaImport.Models;

namespace MediaImport.Views
{
    public sealed partial class GooglePhotoPage : Page
    {
        public object[] UplodingFiles;

        public GooglePhotoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UplodingFiles = (object[])e.Parameter;
            base.OnNavigatedTo(e);
        }
        
        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
            GooglePhotoService.Instance.LogOut();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
        }

        private async void LibraryUploadButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
            foreach (var f in UplodingFiles)
            {
                var file = (StorageFile)f;
                if (file.ContentType.Contains("audio"))
                    continue;
                else await Task.Run(async () => await GooglePhotoService.Instance.UploadFileToLibrary(file));
            }
        }

        private async void AlbumUploadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var album_id = await GooglePhotoService.Instance.CreateNewAlbum();
                foreach (var f in UplodingFiles)
                {
                    var file = (StorageFile)f;
                    if (file.ContentType.Contains("audio"))
                        continue;
                    await Task.Run(async () => await GooglePhotoService.Instance.UploadFileToAlbum(file , album_id));
                }
                Frame.Navigate(typeof(RootPage));
            }
            catch(Exception ex)
            {
                NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                await  NotificateMessageDialog.InformMessage.ShowAsync();
               
            }
        }

    }
}
