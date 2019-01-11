using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MediaImport.Models;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace MediaImport.ViewModels
{
    public class DriveViewModel : INotifyPropertyChanged
    {
        private Drive selectedDrive; // StorageFolder

        private ObservableCollection<Drive> portableDrives;

        public Drive SelectedDrive
        {
            get { return selectedDrive; }
            set
            {
                selectedDrive = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Drive> PortableDrives
        {
            get => portableDrives;
            private set
            {
                portableDrives = value;
                OnPropertyChanged();
            }
        }

        public DriveViewModel()
        {
            PortableDrives = new ObservableCollection<Drive>();
            GenerateKnownFolders();
        }

        private async void GenerateDrives()
        {
            var drives = await Windows.Storage.KnownFolders.RemovableDevices.GetItemsAsync();
            if (portableDrives.Count < drives.Count)
            {
                foreach (var drive in drives)
                {
                    if (PortableDrives.Where(p => p.Name == drive.Name && p.DateCreated == drive.DateCreated).Select(b => b).Count() == 0)
                    {
                        var currentDrive = new Drive ((StorageFolder)drive) { Name = drive.Name, DateCreated = drive.DateCreated};
                        PortableDrives.Add(currentDrive);
                        currentDrive.Files = await ((StorageFolder)drive).GetFilesAsync();
                        currentDrive.Folders = await ((StorageFolder)drive).GetFoldersAsync();
                       
                    }
                }
            }
            
        }

        private async void GenerateKnownFolders()
        {
            PortableDrives.Add(new Drive(KnownFolders.PicturesLibrary)
            {
                Name = KnownFolders.PicturesLibrary.Name,
                DateCreated = KnownFolders.PicturesLibrary.DateCreated,
                Files = await KnownFolders.PicturesLibrary.GetFilesAsync(),
                Folders = await KnownFolders.PicturesLibrary.GetFoldersAsync()
            });

            PortableDrives.Add(new Drive(KnownFolders.MusicLibrary)
            {
                Name = KnownFolders.MusicLibrary.Name,
                DateCreated = KnownFolders.MusicLibrary.DateCreated,
                Files = await KnownFolders.MusicLibrary.GetFilesAsync(),
                Folders = await KnownFolders.MusicLibrary.GetFoldersAsync()
            });

            PortableDrives.Add(new Drive(KnownFolders.VideosLibrary)
            {
                Name = KnownFolders.VideosLibrary.Name,
                DateCreated = KnownFolders.VideosLibrary.DateCreated,
                Files = await KnownFolders.VideosLibrary.GetFilesAsync(),
                Folders = await KnownFolders.VideosLibrary.GetFoldersAsync()
            });

        }

        public void AddNewOne()
        {
            GenerateDrives();
        }
     
        //needs 2 check
        public async void Remove()
        {
            portableDrives.Clear();
            var drives = await Windows.Storage.KnownFolders.RemovableDevices.GetItemsAsync();
            foreach (var drive in drives)
            {
                var currentDrive = new Drive((StorageFolder)drive) { Name = drive.Name, DateCreated = drive.DateCreated };
                currentDrive.Files = await ((StorageFolder)drive).GetFilesAsync();
                currentDrive.Folders = await ((StorageFolder)drive).GetFoldersAsync();
                PortableDrives.Add(currentDrive);  
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
