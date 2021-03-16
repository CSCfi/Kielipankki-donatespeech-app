using System;
using System.Linq;
using Xamarin.Forms;

namespace Recorder.Effects
{
    // Rounder corners effect for Xamarin.Forms.
    // Fashioned after https://medium.com/@anna.domashych/controls-and-layouts-with-rounded-corners-with-xamarin-forms-effect-f6d04444c10a
    public class RoundedCornersEffect : RoutingEffect
    {
        public RoundedCornersEffect() : base("Recorder.Effects.RoundedCornersEffect")
        {
        }

        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.CreateAttached(
                "CornerRadius",
                typeof(int),
                typeof(RoundedCornersEffect),
                0,
                propertyChanged: OnCornerRadiusChanged);

        public static int GetCornerRadius(BindableObject view) =>
            (int)view.GetValue(CornerRadiusProperty);

        public static void SetCornerRadius(BindableObject view, int value) =>
            view.SetValue(CornerRadiusProperty, value);

        private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is View view))
                return;

            var cornerRadius = (int)newValue;
            var effect = view.Effects.OfType<RoundedCornersEffect>().FirstOrDefault();

            if (cornerRadius > 0 && effect == null)
                view.Effects.Add(new RoundedCornersEffect());

            if (cornerRadius == 0 && effect != null)
                view.Effects.Remove(effect);
        }
    }
}
