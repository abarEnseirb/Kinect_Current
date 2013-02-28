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

namespace Kinect_Architecture
{
    public class SkeletonManagement
    {
        public Skeleton skeleton { get; set; }
        public Stickman stickman { get; set; }

        public SkeletonManagement(Skeleton skeleton, Canvas StickMen)
        {
            this.skeleton = skeleton;
            this.stickman = new Stickman(skeleton, StickMen);
        }


        // Vérifie si la distance du skeleton est la plus proche et renvoit l'id le plus proche
        public int NearestID(float nearestDistance2, int nearestId)
        {
            var newNearestId = -1;

            // Find the distance squared.
            var distance2 = (this.skeleton.Position.X * this.skeleton.Position.X) +
                (this.skeleton.Position.Y * this.skeleton.Position.Y) +
                (this.skeleton.Position.Z * this.skeleton.Position.Z);

            // Is the new distance squared closer than the nearest so far?
            if (distance2 < nearestDistance2)
            {
                // Use the new values.
                newNearestId = skeleton.TrackingId;
                nearestDistance2 = distance2;
            }

            if (nearestId != newNearestId)
            {
                nearestId = newNearestId;
            }

            return nearestId;
        }
    }
}
