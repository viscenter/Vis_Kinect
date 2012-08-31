using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using GestureControls.Input;
using System.Diagnostics;


namespace GestureControls.Controls
{
    public class PushButton : KinectButton
    {
        #region Member Variable
        protected double _handDepth;
        #endregion Member Variable


        #region Get/Set and Dep Property
        public double PushThreshold
        {
            get { return (double)GetValue(PushThresholdProperty); }
            set { SetValue(PushThresholdProperty, value); }
        }

        public static readonly DependencyProperty PushThresholdProperty =
            DependencyProperty.Register("PushThreshold", typeof(double), typeof(PushButton), new UIPropertyMetadata(100d));
        #endregion Get/Set and Dep Property


        #region Overrides
        protected override void OnKinectCursorMove(object sender, KinectCursorEventArgs e)
        {
            if (e.Z < _handDepth - PushThreshold)
            {
                RaiseEvent(new RoutedEventArgs(ClickEvent));
            }
        }

        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            _handDepth = e.Z;
        }
        #endregion Overrides
    }
}
