using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MediaImport.Models;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace MediaImport.ViewModels
{
    public class PortableDriveViewModel : INotifyPropertyChanged
    {
        private PortableDrive selectedDrive; // StorageFolder

        private ObservableCollection<PortableDrive> portableDrives;

        public PortableDrive SelectedDrive
        {
            get { return selectedDrive; }
            set
            {
                selectedDrive = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PortableDrive> PortableDrives
        {
            get => portableDrives;
            private set
            {
                portableDrives = value;
                OnPropertyChanged();
            }
        }

        public PortableDriveViewModel()
        {
            PortableDrives = new ObservableCollection<PortableDrive>();
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
                        var currentDrive = new PortableDrive ((StorageFolder)drive) { Name = drive.Name, DateCreated = drive.DateCreated};
                        PortableDrives.Add(currentDrive);
                        currentDrive.Files = await ((StorageFolder)drive).GetFilesAsync();
                        currentDrive.Folders = await ((StorageFolder)drive).GetFoldersAsync();
                    }
                }
            }
           
        }

        private void GenerateKnownFolders()
        {

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
                var currentDrive = new PortableDrive((StorageFolder)drive) { Name = drive.Name, DateCreated = drive.DateCreated };
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
