using System.Windows;


namespace GestureControls.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Controls;
    using GestureControls;
    using System.Windows.Threading;
    using GestureControls.Input;
    using System.Diagnostics;

    public class HoverButton : KinectButton
    {
        #region MemberVariables
        readonly DispatcherTimer _hoverTimer = new DispatcherTimer();
        protected bool _timerEnabled = true;
        #endregion Member Variables


        #region Methods and Dependency Property
        public double HoverInterval
        {
            get { return (double)GetValue(HoverIntervalProperty); }
            set { SetValue(HoverIntervalProperty, value); }
        }

        public static readonly DependencyProperty HoverIntervalProperty =
            DependencyProperty.Register("HoverInterval", typeof(double), typeof(HoverButton), new UIPropertyMetadata(2000d));

        public HoverButton()
        {
            _hoverTimer.Interval = TimeSpan.FromMilliseconds(HoverInterval);
            _hoverTimer.Tick += _hoverTimer_Tick;
            _hoverTimer.Stop();
        }

        void _hoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
        #endregion Methods and Dependency Property


        #region Overrides
        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            if (_timerEnabled)
            {
                _hoverTimer.Interval = TimeSpan.FromMilliseconds(HoverInterval);
                e.KinectCursor.AnimateCursor(HoverInterval);
                _hoverTimer.Start();
            }
        }

        protected override void OnKinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            if (_timerEnabled)
            {
                e.KinectCursor.StopCursorAnimation();
                _hoverTimer.Stop();
            }
        }
        #endregion Overrides
    }
}
