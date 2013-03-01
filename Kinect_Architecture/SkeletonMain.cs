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
    class SkeletonMain
    {
        private SkeletonManagement[] SkeletonManagementData = new SkeletonManagement[0];
        private Curseur curseur;
        private GestureCamera gestureCamera;


        private Canvas StickMen;
        private int nearestId = -1;
        private DateTime highlightTime = DateTime.MinValue;
        private int highlightId = -1;


        private void HighlightSkeleton(Skeleton skeleton)
        {
            // Set the highlight time to be a short time from now.
            highlightTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);

            // Record the ID of the skeleton.
            highlightId = skeleton.TrackingId;
        }

        //For Skeleton
        public void sensor_SkeletonFramesReady(Object sender, SkeletonFrameReadyEventArgs e)
        {
            int i;
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
                            gestureCamera.OnGesture(SkeletonManagementData[i].skeleton);

                            //  nearestId = skeleton.TrackingId;

                            if (ZoomDezoom(skeletonFocus)) //mode zoom
                            {
                                curseur_image.Visibility = Visibility.Hidden;
                                rectZoom.Visibility = Visibility.Visible;

                            }
                            else //mode curseur sinon
                            {
                                rectZoom.Visibility = Visibility.Hidden;
                                curseur.SetCurseur(sensor, SkeletonManagementData[i].skeleton);

                                if (curseur.gridContainsCurseur(global))
                                {
                                    curseur_image.Visibility = Visibility.Visible;

                                    // on peut pas mettre un if pour chaque bouton => reflechir a une boucle qui les parcourt tous ? <=> tableau d'image bouton 

                                    if (curseur.ImageContainsCurseur(select))
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
                                    curseur_image.Visibility = Visibility.Hidden;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
