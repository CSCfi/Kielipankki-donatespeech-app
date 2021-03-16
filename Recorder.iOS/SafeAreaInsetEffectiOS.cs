using System;
using System.Reflection;
using Foundation;
using Recorder.Effects;
using Recorder.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(SafeAreaInsetEffectiOS), "SafeAreaInsetEffect")]
namespace Recorder.iOS
{
    [Preserve(AllMembers = true)]
    public class SafeAreaInsetEffectiOS : PlatformEffect
    {
        private PaddingElement _paddingElement;
        private Thickness _originalPadding;
        private bool _registered;

        protected override void OnAttached()
        {
            _paddingElement = new PaddingElement(Element);
            _originalPadding = _paddingElement.Padding;
            ApplyInsets(_paddingElement, _originalPadding);

            _paddingElement.Element.SizeChanged += ElementSizeChanged;
            _registered = true;
        }

        protected override void OnDetached()
        {
            if (_registered)
            {
                _paddingElement.Element.SizeChanged -= ElementSizeChanged;
                _paddingElement.Padding = _originalPadding;
                _registered = false;
            }
        }

        private void ElementSizeChanged(object sender, System.EventArgs e)
        {
            // Handle orientation changes
            if (_registered)
            {
                ApplyInsets(_paddingElement, _originalPadding);
            }
        }

        private static void ApplyInsets(PaddingElement paddingElement, Thickness defaultPadding)
        {
            var safeInset = GetSafeAreaInset();

            // Get the attached property value so we can apply the appropriate padding
            var insetFlags = SafeAreaInsetEffect.GetInsets(paddingElement.Element);

            // Get the inset modifiers
            var insetModifier = SafeAreaInsetEffect.GetInsetModifier(paddingElement.Element);

            // Combine the safe inset with the view's current padding
            var newPadding = CombineInset(defaultPadding, safeInset, insetFlags, insetModifier);

            paddingElement.Padding = newPadding;
        }

        private static Thickness GetSafeAreaInset()
        {
            var edgeInsets = default(UIEdgeInsets);

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                edgeInsets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets;
            }
            else
            {
                switch (UIApplication.SharedApplication.StatusBarOrientation)
                {
                    case UIInterfaceOrientation.Portrait:
                    case UIInterfaceOrientation.PortraitUpsideDown:
                        // Default padding for older iPhones (top status bar)
                        edgeInsets = new UIEdgeInsets(20, 0, 0, 0);
                        break;
                    default:
                        edgeInsets = new UIEdgeInsets(0, 0, 0, 0);
                        break;
                }
            }

            return new Thickness(edgeInsets.Left, edgeInsets.Top, edgeInsets.Right, edgeInsets.Bottom);
        }

        private static Thickness CombineInset(Thickness defaultPadding, Thickness safeInset, SafeAreaInsets safeAreaInsets, Thickness insetModifier)
        {
            var result = new Thickness(defaultPadding.Left, defaultPadding.Top, defaultPadding.Right, defaultPadding.Bottom);

            if (safeAreaInsets != SafeAreaInsets.None)
            {
                if (safeAreaInsets.HasFlag(SafeAreaInsets.All))
                {
                    result.Left += ModifyInset(safeInset.Left, insetModifier.Left);
                    result.Top += ModifyInset(safeInset.Top, insetModifier.Top);
                    result.Right += ModifyInset(safeInset.Right, insetModifier.Right);
                    result.Bottom += ModifyInset(safeInset.Bottom, insetModifier.Bottom);
                }
                else
                {
                    if (safeAreaInsets.HasFlag(SafeAreaInsets.Left))
                    {
                        result.Left += ModifyInset(safeInset.Left, insetModifier.Left);
                    }
                    if (safeAreaInsets.HasFlag(SafeAreaInsets.Top))
                    {
                        result.Top += ModifyInset(safeInset.Top, insetModifier.Top);
                    }
                    if (safeAreaInsets.HasFlag(SafeAreaInsets.Right))
                    {
                        result.Right += ModifyInset(safeInset.Right, insetModifier.Right);
                    }
                    if (safeAreaInsets.HasFlag(SafeAreaInsets.Bottom))
                    {
                        result.Bottom += ModifyInset(safeInset.Bottom, insetModifier.Bottom);
                    }
                }
            }

            return result;
        }

        private static double ModifyInset(double SafeInsetValue, double Modifier)
        {
            return SafeInsetValue != 0 ? SafeInsetValue + Modifier : SafeInsetValue;
        }

        private class PaddingElement
        {
            private readonly PropertyInfo _paddingInfo;

            public PaddingElement(object e)
            {
                // VisualElement provides the SizeChanged event handler
                if (e is VisualElement ve)
                {
                    Element = ve;

                    // Make sure we've got a view with Padding support
                    var type = Element.GetType();
                    if (type.GetProperty(nameof(Layout.Padding)) is PropertyInfo pi &&
                        pi.GetMethod != null &&
                        pi.SetMethod != null)
                    {
                        _paddingInfo = pi;
                    }
                    else
                    {
                        throw new ArgumentException($"Element must have a {nameof(Layout.Padding)} property with a public getter & setter!");
                    }
                }
                else
                {
                    throw new ArgumentException($"Element must inherit from {nameof(VisualElement)}");
                }
            }

            public VisualElement Element { get; private set; }

            public Thickness Padding
            {
                get => (Thickness)_paddingInfo.GetValue(Element);
                set => _paddingInfo.SetValue(Element, value);
            }
        }
    }
}
