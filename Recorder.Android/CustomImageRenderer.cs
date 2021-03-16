using Android.Content;
using Android.Widget;
using Recorder.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Image), typeof(CustomImageRenderer))]
namespace Recorder.Droid
{
    public class CustomImageRenderer : ImageRenderer
    {
        public CustomImageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.NewElement != null)
            {
                if (e.NewElement.Source is FileImageSource img)
                {
                    if (img.File.StartsWith("bottombar"))
                    {
                        Control.SetScaleType(ImageView.ScaleType.Matrix);
                    }
                }
            }
        }
    }
}
