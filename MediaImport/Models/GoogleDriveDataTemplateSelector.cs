using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaImport.Models
{
    public class GoogleDriveDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if(item is Google.GoogleDriveStorageFolder)
                return FolderTemplate;
            return FolderTemplate;
        }
    }
}
