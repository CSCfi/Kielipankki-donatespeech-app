using System;
using System.IO;
using System.ComponentModel;

using Android.Content;
using Android.Widget;
using ARelativeLayout = Android.Widget.RelativeLayout;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Media;
using Recorder.Models;

[assembly: ExportRenderer(typeof(Recorder.VideoPlayer),
                          typeof(Recorder.Droid.VideoPlayerRenderer))]

namespace Recorder.Droid
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, ARelativeLayout>, MediaPlayer.IOnErrorListener
    {
        VideoView videoView;

        public VideoPlayerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    videoView = new VideoView(Context);

                    // set background to hide default initial black screen until video is prepared
                    UpdateVideoBackground();

                    // Wrap in relative layout to keep video aspect ratio
                    ARelativeLayout relativeLayout = new ARelativeLayout(Context);
                    relativeLayout.AddView(videoView);

                    ARelativeLayout.LayoutParams layoutParams =
                        new ARelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    layoutParams.AddRule(LayoutRules.CenterInParent);
                    videoView.LayoutParameters = layoutParams;

                    videoView.Prepared += OnVideoViewPrepared;
                    videoView.SetOnErrorListener(this);

                    SetNativeControl(relativeLayout);
                }

                UpdateSource();
                UpdatePlayback();

                args.NewElement.ResetToStartRequested += OnResetToStartRequested;
            }

            if (args.OldElement != null)
            {
                args.OldElement.ResetToStartRequested -= OnResetToStartRequested;
            }
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {            
            System.Diagnostics.Debug.WriteLine(string.Format("Failed to play video, MediaError {0}", what.ToString()),
                "Droid.VideoPlayerRenderer");

            // this prevents a built-in alert popup 
            // todo should we have an indicator to user
            return true;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName.Equals(VideoPlayer.PlayProperty.PropertyName))
            {
                UpdatePlayback();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null && videoView != null)
            {
                videoView.Prepared -= OnVideoViewPrepared;
            }

            base.Dispose(disposing);
        }

        void OnVideoViewPrepared(object sender, EventArgs args)
        {
            if (sender is MediaPlayer mediaPlayer)
            {
                if (Element.IsMuted)
                {
                    mediaPlayer.SetVolume(0f, 0f);
                }
            }

            // remove the initial app background
            videoView.SetBackgroundColor(Android.Graphics.Color.Transparent);
        }

        void UpdateVideoBackground()
        {
            if (Element.BackgroundColor != null)
            {
                // this color actually covers the video, so its not really background
                videoView.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            }
        }

        void UpdatePlayback()
        {
            if (Element.Play && !videoView.IsPlaying)
            {
                videoView.Start();                
            }
            else if (!Element.Play && videoView.IsPlaying)
            {
                videoView.StopPlayback();
            }
        }

        void UpdateSource(bool forceSeek = false)
        {
            bool hasSetSource = false; 

            if (Element.Source is UriVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    Android.Net.Uri parsedUri = Android.Net.Uri.Parse(uri);
                    videoView.SetVideoURI(parsedUri);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                string filename = (Element.Source as FileVideoSource).File;

                if (!String.IsNullOrWhiteSpace(filename))
                {
                    videoView.SetVideoPath(filename);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string package = Context.PackageName;
                string path = (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    string filename = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                    string uri = "android.resource://" + package + "/raw/" + filename;
                    videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                    hasSetSource = true;
                }
            }

            if (hasSetSource)
            {
                if (Element.StartTime > 0)
                {
                    videoView.SeekTo(Element.StartTime * 1000);
                }
                else if (forceSeek)
                {
                    videoView.SeekTo(1); // SeekTo(0) does not do anything, so using 1
                }
            }
        }

        private void OnResetToStartRequested(object sender, EventArgs e)
        {
            // re-initialize video source because SeekTo only works in that
            // sequence, and does not work when video stopped, see Android source for VideoView
            UpdateVideoBackground();
            UpdateSource(forceSeek: true);
        }
    }
}
