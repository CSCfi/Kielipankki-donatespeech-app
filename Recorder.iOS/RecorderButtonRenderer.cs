using System;
using System.ComponentModel;
using System.Diagnostics;
using Foundation;
using Recorder.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

//
// Custom renderer for Button on iOS to get button text to wrap
//
// See https://stackoverflow.com/questions/41402020/xamarin-forms-wrap-button-text
//

[assembly: ExportRenderer(typeof(Button), typeof(RecorderButtonRenderer))]
namespace Recorder.iOS
{
    public class RecorderButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.NewElement != null)
            {
                // This prevents font from being clipped at top
                Control.VerticalAlignment = UIControlContentVerticalAlignment.Fill;

                Control.LineBreakMode = UILineBreakMode.WordWrap;

                UpdateText(e.NewElement);
            }
        }

        /**
         * Update the text before the default renderer starts working on it, because it has not assumed
         * that we're messing with the attributed text properties as well.
         * 
         * Note: The text color does not change unless it is being propagated to the NSAttributedString as well.
         * 
         * @see https://github.com/xamarin/Xamarin.Forms/blob/4.6.0/Xamarin.Forms.Platform.iOS/Renderers/ButtonLayoutManager.cs 
         */
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Control != null &&
                Element != null && (
                    e.PropertyName == Button.TextProperty.PropertyName ||
                    e.PropertyName == Button.CharacterSpacingProperty.PropertyName) ||
                    e.PropertyName == Button.TextColorProperty.PropertyName
                )
            {
                // reset the state back to normal for the parent, pretending that we haven't modified it
                Control.SetAttributedTitle(null, UIControlState.Normal);

                // let the ButtonLayoutManager do it's thing
                base.OnElementPropertyChanged(sender, e);

                // add our own text properties
                UpdateText(Element);
            }
            else
            {
                base.OnElementPropertyChanged(sender, e);
            }
        }

        void UpdateText(Button element)
        {
            // Force text uppercase
            Control.SetTitle(element.Text?.ToUpper() ?? "", UIControlState.Normal);

            var title = Control.Title(UIControlState.Normal);
            var range = new NSRange(0, title.Length);
            var newAttrTitle = new NSMutableAttributedString(title);

            // Some text styles are only available as attributed string properties,
            // so we need to mess with them here a bit
            newAttrTitle.AddAttribute(UIStringAttributeKey.ParagraphStyle, new NSMutableParagraphStyle() {
                LineHeightMultiple = 1.2f,
                Alignment = UITextAlignment.Center
            }, range);

            newAttrTitle.AddAttribute(
                UIStringAttributeKey.ForegroundColor,
                Control.TitleColor(UIControlState.Normal),
                range);

            Control.SetAttributedTitle(newAttrTitle, UIControlState.Normal);
        }
    }
}
