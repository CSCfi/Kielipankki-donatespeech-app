using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Recorder
{
    // OnPlatform does not seem to support converting from string to double
    // like this, with two different property data types:
    // <OnPlatform x:Key="BodyFontSize" x:TypeArguments="x:Double" 
    //             iOS="Large" Android="19"  />
    //
    // So, this converter does that specifically
    //
    public class PlatformFontSizeExtension : IMarkupExtension<double>
    {
        public string iOS { get; set; }
        public double iOSFontScale { get; set; } = 1.0;
        public string Android { get; set; }
        public double AndroidFontScale { get; set; } = 1.0;

        private readonly FontSizeConverter fontSizeConverter = new FontSizeConverter();

        public PlatformFontSizeExtension()
        {            
        }

        public double ProvideValue(IServiceProvider serviceProvider)
        {
            string value = Device.RuntimePlatform == Device.iOS ? iOS : Android;
            double scale = Device.RuntimePlatform == Device.iOS ? iOSFontScale : AndroidFontScale;
            return scale * (double)fontSizeConverter.ConvertFromInvariantString(value);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<double>).ProvideValue(serviceProvider);
        }
    }
}
