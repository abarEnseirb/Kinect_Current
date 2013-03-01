using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fizbin.Kinect.Gestures;
using System.ComponentModel;
using Microsoft.Kinect;



    abstract class Gesture
    {
        /* Variables */
        protected GestureController gestureController;
        private string _gesture; // Needed to identify the gesture
        public event PropertyChangedEventHandler PropertyChanged;
        public String gestureKind
        {
            get { return _gesture; }
            set
            {
                if (_gesture == value)
                {
                    return;
                }

                _gesture = value;

                if (this.PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Gesture"));
                }
             }
        }

        /* This method initialises all the gestures needed in the special case */
        abstract public void InitGesture();

        /* This method implements gesture definitions (association between a name and a gesture) */ 
        abstract public void OnGestureRecognized(object sender, GestureEventArgs e);

        /* This method is to update the gestureController */
        public void OnGesture(Skeleton skeleton)
        {
            gestureController.UpdateAllGestures(skeleton);
        }
    }
