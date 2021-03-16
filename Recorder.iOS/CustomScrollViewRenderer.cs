using System;
using Recorder.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ScrollView), typeof(CustomScrollViewRenderer))]
namespace Recorder.iOS
{
    public class CustomScrollViewRenderer : ScrollViewRenderer
    {
        public CustomScrollViewRenderer()
        {
            base.ContentInsetAdjustmentBehavior = UIKit.UIScrollViewContentInsetAdjustmentBehavior.Never;
        }
    }
}
