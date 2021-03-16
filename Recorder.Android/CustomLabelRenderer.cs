using Android.Content;
using Recorder.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Label), typeof(CustomLabelRenderer))]
namespace Recorder.Droid
{
    public class CustomLabelRenderer : LabelRenderer
    {
        public CustomLabelRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.NewElement != null)
            {
                Control.HyphenationFrequency = Android.Text.HyphenationFrequency.Normal;
            }
        }
    }
}