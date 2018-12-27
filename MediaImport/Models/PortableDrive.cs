using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml.Data;
using Windows.Foundation;

namespace MediaImport.Models
{
    public class PortableDrive : INotifyPropertyChanged 
    {
        private string name;

        public DateTimeOffset DateCreated { get; set; }

        private IReadOnlyList<StorageFile> files;
        private IReadOnlyList<StorageFolder> folders;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
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
                OnPropertyChanged("Files");
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
                OnPropertyChanged("Folders");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            throw new NotImplementedException();
        }
        
    }
}
