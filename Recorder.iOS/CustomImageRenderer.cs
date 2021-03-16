using System;
using System.Diagnostics;
using System.IO;
using Recorder.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Image), typeof(CustomImageRenderer))]
namespace Recorder.iOS
{
    public class CustomImageRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.NewElement != null)
            {
                if (e.NewElement.Source is FileImageSource img)
                {
                    if (img.File.StartsWith("bottombar"))
                    {
                        Control.ContentMode = UIKit.UIViewContentMode.Top;
                    }
                }
            }
        }
    }
}
