using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaImport.Models
{
    public class Drive : INotifyPropertyChanged
    {
        private string name;

        public Drive(StorageFolder actualFolderOfDrive)
        {
            ActualFolderOfDrive = actualFolderOfDrive;
        }

        public DateTimeOffset DateCreated { get; set; }

        private IReadOnlyList<StorageFile> files;
        private IReadOnlyList<StorageFolder> folders;

        public StorageFolder ActualFolderOfDrive { get; private set; }


        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<StorageFile> Files
        {
            get
            {
                return files;
            }
            set
            {
                files = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<StorageFolder> Folders
        {
            get
            {
                return folders;
            }
            set
            {
                folders = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        public static explicit operator StorageFolder(Drive pd)
        {
            return pd.ActualFolderOfDrive;
        }

    }
}
