using Microsoft.Toolkit.Services.OneDrive;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaImport.Models
{
    public class OneDriveDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return FolderTemplate;
        }
    }
}