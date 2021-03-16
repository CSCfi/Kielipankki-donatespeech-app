using System.ComponentModel;
using Android.Graphics;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Recorder.Effects;

[assembly: ResolutionGroupName("Recorder.Effects")]
[assembly: ExportEffect(typeof(Recorder.Droid.RoundedCornersEffectDroid), "RoundedCornersEffect")]

namespace Recorder.Droid
{
    public class RoundedCornersEffectDroid : PlatformEffect
    {
        public RoundedCornersEffectDroid()
        {
        }

        protected override void OnAttached()
        {
            try
            {
                PrepareContainer();
                SetCornerRadius();
            }
            catch { }
        }

        protected override void OnDetached()
        {
            try
            {
                Container.OutlineProvider = ViewOutlineProvider.Background;
            }
            catch { }
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == RoundedCornersEffect.CornerRadiusProperty.PropertyName)
                SetCornerRadius();
        }

        private void PrepareContainer()
        {
            Container.ClipToOutline = true;
        }

        private void SetCornerRadius()
        {
            var cornerRadius = RoundedCornersEffect.GetCornerRadius(Element) * GetDensity();
            Container.OutlineProvider = new RoundedOutlineProvider(cornerRadius);
        }

        private static float GetDensity() => MainActivity.Instance.Resources.DisplayMetrics.Density;

        // If we used Plugin.CurrentActivity, the getter would be like this:
        //CrossCurrentActivity.Current.Activity.Resources.DisplayMetrics.Density;

        private class RoundedOutlineProvider : ViewOutlineProvider
        {
            private readonly float _radius;

            public RoundedOutlineProvider(float radius)
            {
                _radius = radius;
            }

            public override void GetOutline(Android.Views.View view, Outline outline)
            {
                outline?.SetRoundRect(0, 0, view.Width, view.Height, _radius);
            }
        }
    }
}
