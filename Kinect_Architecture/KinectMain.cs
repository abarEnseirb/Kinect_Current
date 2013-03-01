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

namespace Kinect_Architecture
{
    class KinectMain
    {
        private KinectSensor sensor;
        private SkeletonMain skeletonMain;


            public KinectMain()
                {
                }

            //When your window is loaded
        private void Window_Loaded(Object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];

            if (this.sensor.Status == KinectStatus.Connected)
            {
                this.sensor.SkeletonStream.Enable();
                this.sensor.SkeletonFrameReady += this.skeletonMain.sensor_SkeletonFramesReady;
                this.sensor.Start();

                //sensor.ElevationAngle = Convert.ToInt32("10");

            }
        }
    }



}
