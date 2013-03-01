using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Microsoft.Samples.Kinect.SwipeGestureRecognizer;
using Microsoft.Kinect;
using Fizbin.Kinect.Gestures.Segments;
using Microsoft.Kinect.Toolkit;
using Fizbin.Kinect.Gestures;
using Microsoft.Samples.Kinect.WpfViewers;
using Kinect_Architecture;



namespace Kinect
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int nearestId = -1;


        private KinectSensor sensor;
        public Skeleton skeletonFocus = new Skeleton();
        private ColorImageFormat lastImageFormat = ColorImageFormat.Undefined;
        private Byte[] pixelData;
        private WriteableBitmap outputImage;
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private int compteur = 0;
        public GestureController gestureController;
        private int xZoomL, xZoomR;

        private SkeletonManagement[] SkeletonManagementData = new SkeletonManagement[0];                // Tableau pour la gestion des SkeletonManagement
        private int i;

        /// <summary>
        /// Time until skeleton ceases to be highlighted.
        /// </summary>
        private DateTime highlightTime = DateTime.MinValue;

        /// <summary>
        /// The ID of the skeleton to highlight.
        /// </summary>
        private int highlightId = -1;

        public MainWindow()
        {
            InitializeComponent();
            // Create the gesture recognizer.
            //this.activeRecognizer = this.CreateRecognizer();

            gestureController = new GestureController();
            gestureController.GestureRecognized += OnGestureRecognized;
            IRelativeGestureSegment[] swipeleftSegments = new IRelativeGestureSegment[3];
            swipeleftSegments[0] = new SwipeLeftSegment1();
            swipeleftSegments[1] = new SwipeLeftSegment2();
            swipeleftSegments[2] = new SwipeLeftSegment3();
            gestureController.AddGesture("SwipeLeft", swipeleftSegments);

            IRelativeGestureSegment[] swiperightSegments = new IRelativeGestureSegment[3];
            swiperightSegments[0] = new SwipeRightSegment1();
            swiperightSegments[1] = new SwipeRightSegment2();
            swiperightSegments[2] = new SwipeRightSegment3();
            gestureController.AddGesture("SwipeRight", swiperightSegments);

            //IRelativeGestureSegment[] zoomInSegments = new IRelativeGestureSegment[3];
            //zoomInSegments[0] = new ZoomSegment1();
            //zoomInSegments[1] = new ZoomSegment2();
            //zoomInSegments[2] = new ZoomSegment3();
            //gestureController.AddGesture("ZoomIn", zoomInSegments);

            //IRelativeGestureSegment[] zoomOutSegments = new IRelativeGestureSegment[3];
            //zoomOutSegments[0] = new ZoomSegment3();
            //zoomOutSegments[1] = new ZoomSegment2();
            //zoomOutSegments[2] = new ZoomSegment1();
            //gestureController.AddGesture("ZoomOut", zoomOutSegments);

            IRelativeGestureSegment[] swipeUpSegments = new IRelativeGestureSegment[3];
            swipeUpSegments[0] = new SwipeUpSegment1();
            swipeUpSegments[1] = new SwipeUpSegment2();
            swipeUpSegments[2] = new SwipeUpSegment3();
            gestureController.AddGesture("SwipeUp", swipeUpSegments);

            IRelativeGestureSegment[] swipeDownSegments = new IRelativeGestureSegment[3];
            swipeDownSegments[0] = new SwipeDownSegment1();
            swipeDownSegments[1] = new SwipeDownSegment2();
            swipeDownSegments[2] = new SwipeDownSegment3();
            gestureController.AddGesture("SwipeDown", swipeDownSegments);
        }

        //When your window is loaded
        private void Window_Loaded(Object sender, RoutedEventArgs e)
        {
            sensor = KinectSensor.KinectSensors[0];

            if (sensor.Status == KinectStatus.Connected)
            {
                sensor.SkeletonStream.Enable();
                //sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                sensor.SkeletonFrameReady += this.sensor_AllFramesReady;
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
                sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorFrameReady);
                sensor.Start();

                //sensor.ElevationAngle = Convert.ToInt32("10");


            }
        }

        // For colorFrame 
        private void sensor_ColorFrameReady(Object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame != null)
                {
                    bool newFormat = this.lastImageFormat != imageFrame.Format;

                    if (newFormat)
                    {
                        this.pixelData = new Byte[imageFrame.PixelDataLength];
                    }

                    imageFrame.CopyPixelDataTo(pixelData);

                    if (newFormat)
                    {
                        this.video.Visibility = Visibility.Visible;
                        this.outputImage = new WriteableBitmap(imageFrame.Width, imageFrame.Height, 0, 0, PixelFormats.Bgr32, null);
                        this.video.Source = this.outputImage;
                    }
                    this.outputImage.WritePixels(new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height), this.pixelData, imageFrame.Width * Bgr32BytesPerPixel, 0);
                    this.lastImageFormat = imageFrame.Format;

                }
            }

        }

        //For Skeleton
        private void sensor_AllFramesReady(Object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {

                    if ((this.SkeletonManagementData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        // Actualise le tableau de SkeletonManagement en fonction du nombre de Skeleton
                        SkeletonManagementData = new SkeletonManagement[skeletonFrame.SkeletonArrayLength];
                    }

                    for (i = 0; i < SkeletonManagementData.Length; i++)
                    {
                        // Initialise les skeletons du tableau skeletonManagementData à partir des skeletons détectés
                        SkeletonManagementData[i] = new SkeletonManagement(skeletonFrame, i, StickMen);
                    }

                    /////////////////////// SELECTION SKELETON //////////////////////

                    // Assume no nearest skeleton and that the nearest skeleton is a long way away.
                    float nearestDistance = (float)double.MaxValue;

                    for (i = 0; i < SkeletonManagementData.Length; i++)
                    {
                        // Only consider tracked skeletons.
                        if (SkeletonManagementData[i].skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (SkeletonManagementData[i].isSkeletonNearest(nearestDistance))
                            {
                                // Récupère la plus proche distance
                                nearestDistance = SkeletonManagementData[i].distance;

                                // Récupère l'id du skeleton correspondant
                                nearestId = SkeletonManagementData[i].skeleton.TrackingId;
                            }
                        }
                    }

                    ///////////////////////// STICKMAN //////////////////////////

                    // Remove any previous skeletons.
                    StickMen.Children.Clear();

                    for (i = 0; i < SkeletonManagementData.Length; i++)
                    {
                        if (SkeletonManagementData[i].skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // Dessine un stickman en fond
                            SkeletonManagementData[i].stickman.DrawStickMan(Brushes.WhiteSmoke, 7);
                        }
                    }

                    for (i = 0; i < SkeletonManagementData.Length; i++)
                    {
                        // Only draw tracked skeletons.
                        if (SkeletonManagementData[i].skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // Pick a brush, Red for a skeleton that has recently gestures, black for the nearest, gray otherwise.
                            var brush = DateTime.UtcNow < highlightTime && SkeletonManagementData[i].skeleton.TrackingId == highlightId ? Brushes.Red :
                                SkeletonManagementData[i].skeleton.TrackingId == nearestId ? Brushes.Black : Brushes.Gray;

                            // Draw the individual skeleton.
                            SkeletonManagementData[i].stickman.DrawStickMan(brush, 3);
                        }

                     
                        ////////////////////////// CURSEUR ///////////////////////////////

                        if (SkeletonManagementData[i].skeleton.TrackingId == nearestId)
                        {
                            skeletonFocus = SkeletonManagementData[i].skeleton;
                            gestureController.UpdateAllGestures(SkeletonManagementData[i].skeleton);

                            //  nearestId = skeleton.TrackingId;

                            if (ZoomDezoom(skeletonFocus)) //mode zoom
                            {
                                curseur.Visibility = Visibility.Hidden;
                                rectZoom.Visibility = Visibility.Visible;

                            }
                            else //mode curseur sinon
                            {
                                rectZoom.Visibility = Visibility.Hidden;

                                ColorImagePoint handColorPoint = SkeletonManagementData[i].HandFocus(sensor, curseur);

                                Canvas.SetLeft(curseur, 2 * (handColorPoint.X) - (curseur.Width / 2));
                                Canvas.SetTop(curseur, 2 * (handColorPoint.Y) - (curseur.Width / 2));
                                Canvas.SetLeft(Rond, 2 * (handColorPoint.X) - (Rond.Width / 2));
                                Canvas.SetTop(Rond, 2 * (handColorPoint.Y) - (Rond.Width / 2));
                                Point rightHand = new Point(2 * handColorPoint.X, 2 * handColorPoint.Y);
                                Point GridRightHand = new Point(handColorPoint.X, handColorPoint.Y);




                                if (gridContainsCurseur(global, GridRightHand))
                                {
                                    curseur.Visibility = Visibility.Visible;

                                    // on peut pas mettre un if pour chaque bouton => reflechir a une boucle qui les parcourt tous ? <=> tableau d'image bouton 

                                    if (ImageContainsCurseur(select, rightHand))
                                    {
                                        Rond.StrokeThickness = compteur * Rond.Width / 100;

                                        if (compteur++ > 100)
                                        {
                                            System.Console.WriteLine("bouton selectionné!!");
                                            compteur = 0;
                                            SkeletCanvas.Background = Brushes.AliceBlue;
                                            Rond.StrokeThickness = 0;

                                        }
                                    }
                                    else
                                    {
                                        compteur = 0;

                                        Rond.StrokeThickness = 0;
                                    }
                                }
                                else
                                {
                                    curseur.Visibility = Visibility.Hidden;
                                }
                            }
                        }
                    }
                }
            }
        }


        // Methode pour un zoom/dezoom 'fluide'
        //////////////////////////////////////////////////////////// Charles bosse dessus
        private bool ZoomDezoom(Skeleton skeleton)
        {

            CoordinateMapper cmLeft = new CoordinateMapper(this.sensor);
            CoordinateMapper cmRight = new CoordinateMapper(this.sensor);
            ColorImagePoint Left = cmLeft.MapSkeletonPointToColorPoint(skeleton.Joints[JointType.HandLeft].Position, ColorImageFormat.RgbResolution1280x960Fps12);
            ColorImagePoint Right = cmRight.MapSkeletonPointToColorPoint(skeleton.Joints[JointType.HandRight].Position, ColorImageFormat.RgbResolution1280x960Fps12);
            // // Coordonnée en Z proche 
            //  if ((skeleton.Joints[JointType.HandRight].Position.Z <= skeleton.Joints[JointType.HandLeft].Position.Z + 1 && skeleton.Joints[JointType.HandRight].Position.Z >= skeleton.Joints[JointType.HandLeft].Position.Z - 1))
            //  {
            // Coordonnée en Y proche 
            if ((Right.Y <= Left.Y + 40 && Right.Y >= Left.Y - 40)) // rajouté une condition sur la position par rapport au corps
            {
                // Coordonnée en X 'centré'
                if (Right.X - 1280 / 2 < 1280 / 2 - Left.X + 40 && Right.X - 1280 / 2 > 1280 / 2 - Left.X - 40)
                {
                    xZoomL = Left.X;
                    xZoomR = Right.X;

                    Canvas.SetLeft(rectZoom, 2 * Left.X);
                    rectZoom.Width = 2 * Right.X - 2 * Left.X;
                    return true;
                }

            }

            xZoomL = -1;
            xZoomR = -1;
            return false;

        }

        // Méthode pour un zoom de la Kinect



        private bool ImageContainsCurseur(Image rect, Point Curseur)
        {
            Double Left = Canvas.GetLeft(rect);
            Double Top = Canvas.GetTop(rect);
            Double Bottom = Top + rect.Height;
            Double Right = Left + rect.Width;
            if (Curseur.X > Left && Curseur.X < Right && Curseur.Y > Top && Curseur.Y < Bottom)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool gridContainsCurseur(Grid grid, Point Curseur)
        {
            Double Left = 0;
            Double Top = 0;
            Double Bottom = Top + grid.ActualHeight;
            Double Right = Left + grid.ActualWidth;

            if (Curseur.X > Left && Curseur.X < Right && Curseur.Y > Top && Curseur.Y < Bottom)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void HighlightSkeleton(Skeleton skeleton)
        {
            // Set the highlight time to be a short time from now.
            highlightTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);

            // Record the ID of the skeleton.
            highlightId = skeleton.TrackingId;
        }

        private string _gesture;
        public String Gesture
        {
            get { return _gesture; }

            private set
            {
                if (_gesture == value)
                    return;

                _gesture = value;

                //    if (this.PropertyChanged != null)
                //        PropertyChanged(this, new PropertyChangedEventArgs("Gesture"));
                //}
            }
        }


        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            switch (e.GestureName)
            {
                case "Menu":
                    Gesture = "Menu";
                    break;
                case "WaveRight":
                    Gesture = "Wave Right";
                    break;
                case "WaveLeft":
                    Gesture = "Wave Left";
                    break;
                case "JoinedHands":
                    Gesture = "Joined Hands";
                    break;
                case "SwipeLeft":
                    Gesture = "Swipe Left";
                    HighlightSkeleton(skeletonFocus);
                    System.Console.WriteLine("Left");
                    SkeletCanvas.Background = Brushes.DarkMagenta;
                    break;
                case "SwipeRight":
                    Gesture = "Swipe Right";
                    HighlightSkeleton(skeletonFocus);
                    SkeletCanvas.Background = Brushes.Lavender;
                    break;
                case "SwipeUp":
                    Gesture = "Swipe Up";
                    HighlightSkeleton(skeletonFocus);
                    SkeletCanvas.Background = Brushes.IndianRed;
                    break;
                case "SwipeDown":
                    Gesture = "Swipe Down";
                    HighlightSkeleton(skeletonFocus);
                    SkeletCanvas.Background = Brushes.Blue;
                    break;
                case "ZoomIn":
                    Gesture = "Zoom In";
                    HighlightSkeleton(skeletonFocus);
                    SkeletCanvas.Background = Brushes.Gold;
                    break;
                case "ZoomOut":
                    Gesture = "Zoom Out";
                    HighlightSkeleton(skeletonFocus);
                    SkeletCanvas.Background = Brushes.Green;
                    break;

                default:
                    break;
            }

        }

    }


}