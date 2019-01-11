using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaImport.Models.Drives
{
    public class DriveTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RemovableDriveTemplate { get; set; }

        public DataTemplate MusicLibraryTemplate { get; set; }

        public DataTemplate PicturesLibraryTemplate { get; set; }

        public DataTemplate VideosLibraryTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if(item is Drive drive)
            {
                if((StorageFolder)drive == KnownFolders.PicturesLibrary)
                {
                    return PicturesLibraryTemplate;
                }
                if ((StorageFolder)drive == KnownFolders.VideosLibrary)
                {
                    return VideosLibraryTemplate;
                }
                if ((StorageFolder)drive == KnownFolders.MusicLibrary)
                {
                    return MusicLibraryTemplate;
                }
                return RemovableDriveTemplate;
            }
            return RemovableDriveTemplate;
        }
    }
}
