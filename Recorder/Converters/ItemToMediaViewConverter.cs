using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

using Recorder.Models;
using Recorder.ViewModels;
using Xamarin.Essentials;

namespace Recorder.Converters
{
    public class ItemToMediaViewConverter : IValueConverter
    {
        double MediaHeight => DeviceDisplay.MainDisplayInfo.GetHeightInSixteenNine();

        public ItemToMediaViewConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScheduleItemViewModel model)
            {
                if (model.IsVideo)
                {
                    return CreateVideo(model);
                }
                else if (model.IsImage || model.IsPromptWithImage)
                {
                    return CreateImage(model, nameof(model.ItemMediaUrl));
                }
                else if (model.IsText)
                {
                    return CreateText(model);
                }
            }

            return null; 
        }

        private View CreateText(ScheduleItemViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ItemMediaUrl))
            {                
                return null; // hide if not defined
            }

            var label = new Label()
            {
                BindingContext = model,
                Padding = 20
            };

            var labelStyle = Application.Current.Resources.GetOrNull("TextScheduleItemLabelStyle");
            if (labelStyle != null)
            {
                label.Style = (Style)labelStyle;
            }

            // text is in the url field
            label.SetBinding(Label.TextProperty, nameof(model.ItemMediaUrl));

            return new Frame()
            {
                Content = label,
                HeightRequest = MediaHeight - 40,
                Padding = 0,
                Margin = new Thickness(40, 20),
                CornerRadius = 50,
                HasShadow = false,
                IsClippedToBounds = true,
            };
        }

        private View CreateVideo(ScheduleItemViewModel model)
        {
            var video = new VideoPlayer()
            {
                BindingContext = model,
                Source = new UriVideoSource(model.Item.Url),
                StartTime = model.Item.StartTime,
                EndTime = model.Item.EndTime,
                Play = model.VideoPlay, // this must be set after start and end time
                HeightRequest = MediaHeight
            };

            var backgroundColor = Application.Current.Resources.GetOrNull("AppBackgroundColor");
            if (backgroundColor != null)
            {
                video.BackgroundColor = (Color)backgroundColor;
            }

            // binding starts video when recording starts and stops when recording stops,
            // and this is also updated if app goes to background
            video.SetBinding(VideoPlayer.PlayProperty, nameof(model.VideoPlay));

            if (model.IsRecordingEnabled)
            {
                video.IsMuted = true;

                // this event handler is removed in ScheduleItemViewModel.ClearAfterDisplay
                model.VideoReset += (o, e) => video.Reset();               

                var image = CreateImage(model, nameof(model.VideoItemImageUrl));

                // overlay image on top of video, so if image is defined it will cover
                // video and we dont need to show/hide either
                Grid grid = new Grid();
                grid.Children.Add(video);
                grid.Children.Add(image);
                return grid;
            }
            else
            {
                video.IsMuted = false;
                return video;
            }
        }

        private Image CreateImage(ScheduleItemViewModel model, string urlPropertyName)
        {
            var image = new Image()
            {
                BindingContext = model,
                HeightRequest = MediaHeight,
                Aspect = Aspect.AspectFill
            };

            image.SetBinding(Image.SourceProperty, urlPropertyName,
                BindingMode.Default, new StringToImageSourceConverter());

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }        
    }
}
