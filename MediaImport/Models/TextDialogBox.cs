using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace MediaImport.Models
{
    class TextDialogBox
    {
       public  static async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox { AcceptsReturn = false, Height = 32 };

            ContentDialog dialog = new ContentDialog
            {
                Content = inputTextBox,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel"
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return inputTextBox.Text;
            }
            else
            {
                return null;
            }
        }
    }

    public static class NotificateMessageDialog
    {
        public static MessageDialog InformMessage { get; set; }
    }
}
