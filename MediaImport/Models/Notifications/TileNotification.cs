using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library
using Windows.UI.StartScreen;

namespace MediaImport.Models.Notifications
{
    public class TileNotification
    {
       // readonly XmlDocument TileXml;


        public TileNotification()
        {
          //  TileXml = tileXml;
        }

        public void ChangeTileContent( string first,string second)
        {
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                             Children =
                                    {
                                        new AdaptiveText()
                                        {
                                            Text = first
                                        },

                                        new AdaptiveText()
                                        {
                                            Text = second,
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                    }
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = first,
                        HintStyle = AdaptiveTextStyle.Subtitle
                    },

                    new AdaptiveText()
                    {
                        Text = second,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },
                    
                }
                        }
                    }
                }
            };
            var notification = new Windows.UI.Notifications.TileNotification(content.GetXml());
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(2);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);

            if (SecondaryTile.Exists("MySecondaryTile"))
            {
                // Get its updater
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile("MySecondaryTile");

                // And send the notification
                updater.Update(notification);
            }
        }

    }
}
