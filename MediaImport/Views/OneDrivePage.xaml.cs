using Microsoft.Graph;
using Microsoft.Toolkit.Services.OneDrive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using MediaImport.Views;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Core;
using MediaImport.Models;

namespace MediaImport.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OneDrivePage : Page
    {
        private OneDriveStorageFolder rootFolder = null;
        private OneDriveStorageFolder currentFolder = null;


        public object[] UplodingFiles;

        public OneDrivePage()
        {
            this.InitializeComponent();
            InitializeUsing();
        }

        private async void InitializeUsing()
        {
            rootFolder = currentFolder = await OneDriveService.Instance.RootFolderForMeAsync();
            OneDriveFoldersListView.ItemsSource = await rootFolder.GetFoldersAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UplodingFiles = (object[])e.Parameter;
            base.OnNavigatedTo(e);
        }

        private async void  RenameButton_Click(object sender, RoutedEventArgs e)
        {
            await RenameAsync((OneDriveStorageItem)((AppBarButton)e.OriginalSource).DataContext);
            OneDriveFoldersListView.ItemsSource = await currentFolder.GetFoldersAsync();
        }

        private async  void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await DeleteAsync((OneDriveStorageItem)((AppBarButton)e.OriginalSource).DataContext);
            OneDriveFoldersListView.ItemsSource = await currentFolder.GetFoldersAsync();
        }

        private void OneDriveFoldersListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var o = OneDriveFoldersListView.SelectedItem as OneDriveStorageFolder;
            NavigateToFolderAsync(o);
        }

        private async void NavigateToFolderAsync(OneDriveStorageItem item)
        {
           
            if (item.IsFolder())
            {
                try
                {
                    var _currentFolder = await currentFolder.GetFolderAsync(item.Name);
                    OneDriveFoldersListView.ItemsSource = await _currentFolder.GetFoldersAsync();
                    currentFolder = _currentFolder;
                }
                catch (ServiceException ex)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }        
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigateBackAsync();
        }

        private async void NavigateBackAsync()
        {
          
            if (currentFolder != null)
            {
                OneDriveStorageFolder _currentFolder = null;
                try
                {
                    if (!string.IsNullOrEmpty(currentFolder.Path))
                    {
                        _currentFolder = await rootFolder.GetFolderAsync(currentFolder.Path);
                    }
                    else
                    {
                        _currentFolder = rootFolder;
                    }

                    OneDriveFoldersListView.ItemsSource = await _currentFolder.GetFoldersAsync();
                    currentFolder = _currentFolder;
                }
                catch (ServiceException ex)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
                
            }
        }
        
        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await NewFolderAsync(currentFolder);
            OneDriveFoldersListView.ItemsSource = await currentFolder.GetFoldersAsync();
        }

        public static async Task NewFolderAsync(OneDriveStorageFolder folder)
        {
            if (folder != null)
            {
                try
                {
                    string newFolderName = await Models.TextDialogBox.InputTextDialogAsync("New Folder Name");
                    if (!string.IsNullOrEmpty(newFolderName))
                    {
                        await folder.StorageFolderPlatformService.CreateFolderAsync(newFolderName, CreationCollisionOption.GenerateUniqueName);
                    }
                }
                catch (ServiceException ex)
                {
                    NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                    await NotificateMessageDialog.InformMessage.ShowAsync();
                }
            }
        }

        public static async Task RenameAsync(OneDriveStorageItem itemToRename)
        {
            try
            { 
                string newName = await Models.TextDialogBox.InputTextDialogAsync("New Name");
                if (!string.IsNullOrEmpty(newName))
                {
                    await itemToRename.RenameAsync(newName);
                }
            }
            catch (ServiceException graphEx)
            {
                NotificateMessageDialog.InformMessage = new MessageDialog(graphEx.Message);
                await NotificateMessageDialog.InformMessage.ShowAsync();
            }
            catch (Exception ex)
            {
                NotificateMessageDialog.InformMessage = new MessageDialog(ex.Message);
                await NotificateMessageDialog.InformMessage.ShowAsync();
            }
        }

        private async Task DeleteAsync(Microsoft.Toolkit.Services.OneDrive.OneDriveStorageItem itemToDelete)
        {
            MessageDialog messageDialog = new MessageDialog($"Are you sure you want to delete '{itemToDelete.Name}'", "Delete");
            messageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(async (cmd) =>
            {
                try
                {
                    await itemToDelete.DeleteAsync();
                }
                catch (ServiceException ex)
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

        private async void SelectSaveFolder_Click(object sender, RoutedEventArgs e)
        {      
            Frame.Navigate(typeof(RootPage));

            foreach (var f in UplodingFiles)
            {
                var file = (StorageFile)f;
                var prop = await file.GetBasicPropertiesAsync();
                if (!(OneDriveFoldersListView.SelectedItem is OneDriveStorageFolder destinationFolder))
                    destinationFolder = currentFolder;
                if (prop.Size >= 40000)//bytes
                {
                    await UploadLargeFileAsync(destinationFolder ,file);
                }
                else
                {
                    await UploadSimpleFileAsync(destinationFolder , file);
                }
            }
        }

        public static async Task UploadSimpleFileAsync(OneDriveStorageFolder folder , StorageFile file)
        {
            if (folder != null)
            {
                if (file != null)//checking my files
                {
                    using (var localStream = await file.OpenReadAsync())
                    {
                        var fileCreated = await folder.StorageFolderPlatformService.CreateFileAsync(file.Name, CreationCollisionOption.GenerateUniqueName, localStream);
                    }
                }
            }
        }

        public static async Task UploadLargeFileAsync(OneDriveStorageFolder folder, StorageFile file)
        {
            if (folder != null)
            {
                if (file != null)
                {
                    using (var localStream = await file.OpenReadAsync())
                    {
                        var largeFileCreated = await folder.StorageFolderPlatformService.UploadFileAsync(file.Name, localStream, CreationCollisionOption.GenerateUniqueName, 320 * 1024);
                    }
                }
            }
        }
        
        private void BCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RootPage));
        }
        

        private async void BLogOut_Click(object sender, RoutedEventArgs e)
        {
            await OneDriveService.Instance.LogoutAsync();
            Frame.Navigate(typeof(RootPage));
        }
    }
}
