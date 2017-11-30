using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures.Samples.Camera3D;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App2
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }
        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.gestureService = GesturesServiceEndpointFactory.Create();
            await this.gestureService.ConnectAsync();

            this.smoother = new IndexSmoother();
            this.smoother.SmoothedPositionChanged += OnSmoothedPositionChanged;

            await this.gestureService.RegisterToSkeleton(this.OnSkeletonDataReceived);
        }
        void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.canvasSize = e.NewSize;
        }
        void OnSkeletonDataReceived(object sender, HandSkeletonsReadyEventArgs e)
        {
            var right = e.HandSkeletons.FirstOrDefault(h => h.Handedness == Hand.RightHand);

            if (right != null)
            {
                this.smoother.Smooth(right);
            }
        }
        async void OnSmoothedPositionChanged(object sender, SmoothedPositionChangeEventArgs e)
        {
            // AFAIK, the positions here are defined in terms of millimetres and range
            // -ve to +ve with 0 at the centre.

            // I'm unsure what range the different cameras have in terms of X,Y,Z and
            // so I've made up my own range which is X from -200 to 200 and Y from
            // -90 to 90 and that seems to let me get "full scale" on my hand 
            // movements.

            // I'm sure there's a better way. X is also reversed for my needs so I
            // went with a * -1.

            var xPos = Math.Clamp(e.SmoothedPosition.X * - 1.0, 0 - XRANGE, XRANGE);
            var yPos = Math.Clamp(e.SmoothedPosition.Y, 0 - YRANGE, YRANGE);
            xPos = (xPos + XRANGE) / (2.0d * XRANGE);
            yPos = (yPos + YRANGE) / (2.0d * YRANGE);

            await this.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    this.marker.Visibility = Visibility.Visible;

                    var left = (xPos * this.canvasSize.Width);
                    var top = (yPos * this.canvasSize.Height);

                    Canvas.SetLeft(this.marker, left - (this.marker.Width / 2.0));
                    Canvas.SetTop(this.marker, top - (this.marker.Height / 2.0));
                    this.txtDebug.Text = $"{left:N1},{top:N1}";
                }

            );
        }
        static readonly double XRANGE = 200;
        static readonly double YRANGE = 90;
        Size canvasSize;
        GesturesServiceEndpoint gestureService;
        IndexSmoother smoother;
    }
}