using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaImport.Models.Notifications
{
    public enum TileType { ImageTile, TextTile };

    public class TileNotification
    {
        public TileNotification()
        {
        }

        public void ChangeTileContent(TileType tileType, string firstTitle, string secondTitle = "", string imageSource = "")
        {
            if (tileType == TileType.TextTile && !(string.IsNullOrEmpty(secondTitle) && string.IsNullOrWhiteSpace(secondTitle)))
            {
                TextTileContent(firstTitle, secondTitle);
            }
            if (tileType == TileType.ImageTile && !(string.IsNullOrEmpty(imageSource) && string.IsNullOrWhiteSpace(imageSource)))
            {
                ImageTileContent(firstTitle, imageSource);
            }
        }

        public void ImageTileContent(string first, string source)
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

                                        new AdaptiveImage()
                                        {
                                            Source = source
                                        }
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
                                new AdaptiveImage()
                                {
                                     Source = source
                                }
                            }
                        }
                    },
                    TileLarge = new TileBinding()
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
                                new AdaptiveImage()
                                {
                                     Source = source
                                }
                            }
                        }
                    }
                }
            };
            ShowTile(content);

        }


        public void TextTileContent(string first, string second)
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
                                        }
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
                                }
                            }
                        }
                    }
                }
            };

            ShowTile(content);
        }

        public void ShowTile(TileContent tileContent)
        {
            var notification = new Windows.UI.Notifications.TileNotification(tileContent.GetXml());
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(2);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);

            if (SecondaryTile.Exists("MySecondaryTile"))
            {
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile("MySecondaryTile");

                updater.Update(notification);
            }
        }

    }
}
