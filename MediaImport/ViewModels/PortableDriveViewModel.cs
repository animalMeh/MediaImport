using System;
using System.Collections.Generic;
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
        private PortableDrive selectedDrive;

        private ObservableCollection<PortableDrive> portableDrives;

        public PortableDrive SelectedDrive
        {
            get { return selectedDrive; }
            set
            {
                selectedDrive = value;
                OnPropertyChanged("SelectedDrive");
            }
        }

        public ObservableCollection<PortableDrive> PortableDrives
        {
            get => portableDrives;
            private set
            {
                portableDrives = value;
                OnPropertyChanged("PortableDrivers");
            }
        }

        public PortableDriveViewModel()
        {
            PortableDrives = new ObservableCollection<PortableDrive>();
        }

        private async void GenerateDrives()
        {
            IReadOnlyList<StorageFile> files = null;
            IReadOnlyList<StorageFolder> foldersIn = null;

            var drives = await Windows.Storage.KnownFolders.RemovableDevices.GetItemsAsync();
            
            if (portableDrives.Count < drives.Count)
            {
                foreach (var drive in drives)
                {
                    if (PortableDrives.Where(p => p.Name == drive.Name && p.DateCreated == drive.DateCreated).Select(b => b).Count() != 0)
                    {
                    }
                    else
                    {
                        var currentDrive = new PortableDrive { Name = drive.Name, Files = files, Folders = foldersIn, DateCreated = drive.DateCreated };
                        PortableDrives.Add(currentDrive);
                        currentDrive.Files = await ((StorageFolder)drive).GetFilesAsync();
                        currentDrive.Folders = await ((StorageFolder)drive).GetFoldersAsync();
                    }
                }
            }
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
                var currentDrive = new PortableDrive { Name = drive.Name, Files = await ((StorageFolder)drive).GetFilesAsync(), Folders = await ((StorageFolder)drive).GetFoldersAsync(), DateCreated = drive.DateCreated };
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
