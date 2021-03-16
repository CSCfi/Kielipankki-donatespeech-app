using System;
using System.ComponentModel;
using CoreAnimation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Recorder.Effects;
using Recorder.iOS;

[assembly: ResolutionGroupName("Recorder.Effects")]
[assembly: ExportEffect(typeof(Recorder.iOS.RoundedCornersEffectiOS), "RoundedCornersEffect")]

namespace Recorder.iOS
{
    public class RoundedCornersEffectiOS : PlatformEffect
    {
        public RoundedCornersEffectiOS()
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
                Container.Layer.CornerRadius = new nfloat(0);
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
            Container.ClipsToBounds = true;
            Container.Layer.AllowsEdgeAntialiasing = true;
            Container.Layer.EdgeAntialiasingMask = CAEdgeAntialiasingMask.All;
        }

        private void SetCornerRadius()
        {
            var cornerRadius = RoundedCornersEffect.GetCornerRadius(Element);
            Container.Layer.CornerRadius = new nfloat(cornerRadius);
        }
    }
}
