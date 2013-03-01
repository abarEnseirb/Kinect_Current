using System;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Microsoft.Kinect;

public class Curseur
{

    private Grid global;
    private List<Image> imageButtons;
    private Image curseur;
    private Ellipse rond;


	public Curseur(Grid global, Image curseur, Ellipse rond)
	{
        this.global = global;
        this.curseur = curseur;
        this.rond = rond;
        this.imageButtons = new List<Image>();
	}

 
    /* This methode select the focus hand */
    private JointType HandFocus(Skeleton skeleton)
    {
        if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HandLeft].Position.Y)
        {
            curseur.Source = new BitmapImage(new Uri("Ressources/Images/hand_r.png", UriKind.Relative));
            return JointType.HandRight;
        }
        else
        {
            curseur.Source = new BitmapImage(new Uri("Ressources/Images/hand_l.png", UriKind.Relative));
            return JointType.HandLeft;
        }
    }

    /* This methode set the position of the curseur */
    public void SetCurseur(KinectSensor sensor, Skeleton skeleton) 
    {
        CoordinateMapper cm = new CoordinateMapper(sensor);
        ColorImagePoint handColorPoint = cm.MapSkeletonPointToColorPoint(skeleton.Joints[HandFocus(skeleton)].Position, ColorImageFormat.RgbResolution1280x960Fps12);

        Canvas.SetLeft(this.curseur, 2 * (handColorPoint.X) - (this.curseur.Width / 2));
        Canvas.SetTop(this.curseur, 2 * (handColorPoint.Y) - (this.curseur.Width / 2));
        Canvas.SetLeft(this.rond, 2 * (handColorPoint.X) - (this.rond.Width / 2));
        Canvas.SetTop(this.rond, 2 * (handColorPoint.Y) - (this.rond.Width / 2));
    }



    /* This methode checks if the curseur is on a image */
    public bool ImageContainsCurseur(Image imageButton)
    {
        Double Left = Canvas.GetLeft(imageButton);
        Double Top = Canvas.GetTop(imageButton);
        Double Bottom = Top + imageButton.Height;
        Double Right = Left + imageButton.Width;

        if ((Canvas.GetLeft(this.curseur) + (this.curseur.Width / 2)) > Left && (Canvas.GetLeft(this.curseur) + (this.curseur.Width / 2)) < Right && (Canvas.GetTop(this.curseur) + (this.curseur.Width / 2)) > Top && (Canvas.GetTop(this.curseur) + (this.curseur.Width / 2)) < Bottom)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool gridContainsCurseur(Grid grid)
    {
        Double Left = 0;
        Double Top = 0;
        Double Bottom = Top + grid.ActualHeight;
        Double Right = Left + grid.ActualWidth;

        if ((Canvas.GetLeft(this.curseur) + (this.curseur.Width / 2)) > Left && (Canvas.GetLeft(this.curseur) + (this.curseur.Width / 2)) < Right && (Canvas.GetTop(this.curseur) + (this.curseur.Width / 2)) > Top && (Canvas.GetTop(this.curseur) + (this.curseur.Width / 2)) < Bottom)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}
