using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using AVFoundation;
using AVKit;
using Foundation;
using Recorder.Models;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Recorder.VideoPlayer),
                          typeof(Recorder.iOS.VideoPlayerRenderer))]


namespace Recorder.iOS
{
    public class VideoPlayerRenderer: ViewRenderer<VideoPlayer, UIView>
    {
        AVPlayer player;
        AVPlayerItem playerItem;
        AVPlayerViewController _playerViewController;       // solely for ViewController property

        public override UIViewController ViewController => _playerViewController;

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    player = new AVPlayer();
                    _playerViewController = new AVPlayerViewController
                    {
                        Player = player,
                        ShowsPlaybackControls = false
                    };
                    SetNativeControl(_playerViewController.View);
                }

                UpdateBackground();
                UpdateSource();
                UpdatePlayback();

                args.NewElement.ResetToStartRequested += OnResetToStartRequested;
            }

            if (args.OldElement != null)
            {
                args.OldElement.ResetToStartRequested -= OnResetToStartRequested;
            }
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
            base.Dispose(disposing);

            if (player != null)
            {
                player.ReplaceCurrentItemWithPlayerItem(null);
            }
        }

        void UpdateBackground()
        {
            if (Element.BackgroundColor != null)
            {
                _playerViewController.View.BackgroundColor = Element.BackgroundColor.ToUIColor();
            }
        }

        void UpdatePlayback()
        {
            Debug.WriteLine($"UpdatePlayback: Element.Play = {Element.Play}, player.Rate = {player.Rate}");
            if (Element.Play && player.Rate == 0)
            {
                player.Play();
            }
            else if (!Element.Play && player.Rate > 0)
            {
                player.Pause();
            }
        }

        void UpdateSource()
        {
            AVAsset asset = null;

            if (Element.Source is UriVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    asset = AVAsset.FromUrl(new NSUrl(uri));
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                string uri = (Element.Source as FileVideoSource).File;

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    asset = AVAsset.FromUrl(new NSUrl(uri));
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string path = (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    string directory = Path.GetDirectoryName(path);
                    string filename = Path.GetFileNameWithoutExtension(path);
                    string extension = Path.GetExtension(path).Substring(1);
                    NSUrl url = NSBundle.MainBundle.GetUrlForResource(filename, extension, directory);
                    asset = AVAsset.FromUrl(url);
                }
            }

            if (asset != null)
            {
                playerItem = new AVPlayerItem(asset);
            }
            else
            {
                playerItem = null;
            }

            player.ReplaceCurrentItemWithPlayerItem(playerItem);

            if (playerItem != null)
            {
                player.Muted = Element.IsMuted;
                if (Element.StartTime > 0)
                {
                    player.Seek(StartCMTime);
                }
            }
        }

        private void OnResetToStartRequested(object sender, EventArgs e)
            => player.Seek(StartCMTime);

        private CoreMedia.CMTime StartCMTime
            => new CoreMedia.CMTime(Element.StartTime, 1);
    }
}
