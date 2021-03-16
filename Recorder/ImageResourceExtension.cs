using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Recorder
{
    /// <summary>
    /// Custom XAML markup extension can be written to load images using a Resource ID specified in XAML.
    /// Needed because there is no built-in type converter from string to ResourceImageSource, so local
    /// resource images cannot be natively loaded by XAML.
    /// See https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/images?tabs=macos#embedded-images
    /// </summary>
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }

            // Do your translation lookup here, using whatever method you require
            var imageSource = ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);

            return imageSource;
        }
    }
}
