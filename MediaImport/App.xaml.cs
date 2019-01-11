using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using Windows.Storage;

namespace MediaImport
{
    sealed partial class App : Application
    {
       
        public static MediaImport.Models.Notifications.TileNotification AppTile
            = new Models.Notifications.TileNotification();
      
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            AppCenter.Start("feedb1b8-2df9-4c08-81f5-ac754ddf6fa9", typeof(Analytics));
        }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            // base.OnFileActivated(args);
            Receiving(await args.ShareOperation.Data.GetStorageItemsAsync());
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Receiving(args.Files);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(Views.MainPage), e.Arguments);
                }
          
                Window.Current.Activate();
            }
        }

        private void Receiving(IReadOnlyList<IStorageItem> items)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            rootFrame.Navigate(typeof(Views.RecievedData), items);
            Window.Current.Activate();
        }

       

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            var files = await localFolder.GetFilesAsync();
            try
            {
                foreach (var f in files)
                {
                    await f.DeleteAsync();
                }
            }
            catch (Exception exc) { }
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
