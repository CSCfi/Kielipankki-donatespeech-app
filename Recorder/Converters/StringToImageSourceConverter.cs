using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace Recorder.Converters
{
    public class StringToImageSourceConverter : IValueConverter
    {
        public StringToImageSourceConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var url = (value as string).Trim();

                try
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        return null;
                    }
                    else if (url.StartsWith("http:") || url.StartsWith("https:"))
                    {
                        return ImageSource.FromUri(new Uri(url));
                    }
                    else
                    {
                        string processedLocalFileUrl = url.Replace('/', '.');
                        return ImageSource.FromResource(
                            $"Recorder.Images.{processedLocalFileUrl}",
                            typeof(StringToImageSourceConverter).GetTypeInfo().Assembly);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format("Failed to convert url: {0}", url), nameof(StringToImageSourceConverter));
                    Debug.WriteLine(e.ToString(), nameof(StringToImageSourceConverter));
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
