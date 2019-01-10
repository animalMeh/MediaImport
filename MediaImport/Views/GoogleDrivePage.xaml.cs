using MediaImport.Models;
using MediaImport.Models.Google;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace MediaImport.Views
{
    public sealed partial class GoogleDrivePage : Page
    {
        private GoogleDriveStorageFolder rootFolder = null;
        private GoogleDriveStorageFolder currentFolder = null;

        public object[] UplodingFiles;

        public GoogleDrivePage()
        {
            this.InitializeComponent();
            InitializeFolders();
        }
        
        public async  void InitializeFolders()
        {
            rootFolder = currentFolder = GoogleDriveService.Instance.RootFolder;
            GoogleDriveFoldersListView.ItemsSource = await rootFolder.GetFoldersAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UplodingFiles = (object[])e.Parameter;
            base.OnNavigatedTo(e);
        }

        private async void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            await RenameAsync((GoogleDriveStorageFolder)((AppBarButton)e.OriginalSource).DataContext);
            GoogleDriveFoldersListView.ItemsSource = await currentFolder.GetFoldersAsync();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await DeleteAsync((GoogleDriveStorageFolder)((AppBarButton)e.OriginalSource).DataContext);
            GoogleDriveFoldersListView.ItemsSource = await currentFolder.GetFoldersAsync();
        }

        private void BCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
        }

        private void BLogOut_Click(object sender, RoutedEventArgs e)
        {
            GoogleDriveService.Instance.LogOut();
            Frame.Navigate(typeof(RootPage));
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigateBackAsync();
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await NewFolderAsync(currentFolder);
            GoogleDriveFoldersListView.ItemsSource = await currentFolder.GetFoldersAsync();
        }

        private async void NavigateBackAsync()
        {
          
            if (currentFolder != null)
            {
                GoogleDriveStorageFolder _currentFolder = null;
                try
                {
                    if (currentFolder.Parent != null)
                    {
                        _currentFolder = await GoogleDriveStorageFolder.GetFolderAsync(currentFolder.Parent);//parent
                    }
                    else
                    {
                        _currentFolder = rootFolder;
                    }
                    GoogleDriveFoldersListView.ItemsSource = await _currentFolder.GetFoldersAsync();
                    currentFolder = _currentFolder;
                }
                catch (Exception ex)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
            }
        }

        private void GoogleDriveFoldersListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var o = GoogleDriveFoldersListView.SelectedItem as GoogleDriveStorageFolder;
            NavigateToFolderAsync(o);
        }

        private async void NavigateToFolderAsync(GoogleDriveStorageFolder item)
        {
           
            try
                {
                    var _currentFolder = await GoogleDriveStorageFolder.GetFolderAsync(item.Id);
                    GoogleDriveFoldersListView.ItemsSource = await _currentFolder.GetFoldersAsync();
                    currentFolder = _currentFolder;
                }
                catch (Exception ex)
                {
                NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                await NotificateMessageDialog.InformMessage.ShowAsync();
                }
        }

        private async void SelectSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
            foreach (var f in UplodingFiles)
            {
                var file = (StorageFile)f;
                var prop = await file.GetBasicPropertiesAsync();
                if (!(GoogleDriveFoldersListView.SelectedItem is GoogleDriveStorageFolder destinationFolder))
                    destinationFolder = currentFolder;
                await Task.Run(async () => await destinationFolder.UploadFile(file));
            }
        }

        public static async Task NewFolderAsync(GoogleDriveStorageFolder folder)
        {
            if (folder != null)
            {
                try
                {
                    string newFolderName = await Models.TextDialogBox.InputTextDialogAsync("New Folder Name");
                    if (!string.IsNullOrEmpty(newFolderName))
                    {
                        await folder.CreateNewFolder(newFolderName);
                    }
                }
                catch (Exception ex)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
            }
        }

        public static async Task RenameAsync(GoogleDriveStorageFolder itemToRename)
        {
            try
            {
                string newName = await Models.TextDialogBox.InputTextDialogAsync("New Name");
                if (!string.IsNullOrEmpty(newName))
                {
                    itemToRename.Rename(newName);
                }
            }
           
            catch (Exception ex)
            {
                NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                await NotificateMessageDialog.InformMessage.ShowAsync();
            }
        }

        private async Task DeleteAsync(GoogleDriveStorageFolder folder)
        {
            MessageDialog messageDialog = new MessageDialog($"Are you sure you want to delete '{folder.Title}'", "Delete");
            messageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(async (cmd) =>
            {
                try
                {
                    folder.Delete();
                }
                catch (Exception ex)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }

            })));

            messageDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler((cmd) => { return; })));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            var command = await messageDialog.ShowAsync();
        }
    }
}
