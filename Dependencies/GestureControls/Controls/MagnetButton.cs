using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GestureControls.Input;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Controls;


namespace GestureControls.Controls
{
    public class MagnetButton : HoverButton
    {
        #region Member Variables
        protected bool _isLockedOn = true;
        private KinectCursorEventArgs _lastPointDetected;
        Storyboard move;
        public static readonly RoutedEvent KinectCursorLockEvent = KinectInput.KinectCursorLockEvent.AddOwner(typeof(MagnetButton));
        public static readonly RoutedEvent KinectCursorUnlockEvent = KinectInput.KinectCursorUnlockEvent.AddOwner(typeof(MagnetButton));
        #endregion Member Variables


        #region Constructors and Events
        public MagnetButton() { }

        public event KinectCursorEventHandler KinectCursorLock
        {
            add { base.AddHandler(KinectCursorLockEvent, value); }
            remove { base.RemoveHandler(KinectCursorLockEvent, value); }
        }

        public event KinectCursorEventHandler KienctCursorUnlock
        {
            add { base.AddHandler(KinectCursorUnlockEvent, value); }
            remove { base.RemoveHandler(KinectCursorUnlockEvent, value); }
        }
        #endregion COnstructors and Overrides


        #region Gets and Sets
        public double LockInterval
        {
            get { return (double)GetValue(LockIntervalProperty); }
            set { SetValue(LockIntervalProperty, value); }
        }

        public double UnlockInterval
        {
            get { return (double)GetValue(UnlockIntervalProperty); }
            set { SetValue(UnlockIntervalProperty, value); }
        }

        public double LockXOffsetFromCenter
        {
            get { return (double)GetValue(LockXOffsetFromCenterProperty); }
            set { SetValue(LockXOffsetFromCenterProperty, value); }
        }

        public double LockYOffsetFromCenter
        {
            get { return (double)GetValue(LockYOffsetFromCenterProperty); }
            set { SetValue(LockYOffsetFromCenterProperty, value); }
        }
        #endregion Gets and Sets


        #region Dependecy Properties
        public static readonly DependencyProperty LockIntervalProperty =
            DependencyProperty.Register("LockInterval", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(200d));

        public static readonly DependencyProperty UnlockIntervalProperty =
            DependencyProperty.Register("UnlockInterval", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(80d));

        public static readonly DependencyProperty LockXOffsetFromCenterProperty =
            DependencyProperty.Register("LockXOffsetFromCenter", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(0d));

        public static readonly DependencyProperty LockYOffsetFromCenterProperty =
            DependencyProperty.Register("LockYOffsetFromCenter", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(0d));
        #endregion Dependency Properties


        #region Methods and Overrides
        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            if (this.Opacity == 0)
                return;
            if (!_isLockedOn)
                return;
            var rootVisual = FindAncestor<Window>(this);
            var point = this.TransformToAncestor(rootVisual).Transform(new Point(0, 0));

            // Extract button position
            var x = point.X + this.ActualWidth / 2;
            var y = point.Y + this.ActualHeight / 2;

            var cursor = e.Cursor;
            cursor.UpdateCursor(new Point(e.X, e.Y), true);

            // Finds the target position
            Point lockPoint = new Point(x - cursor.CursorVisual.ActualWidth / 2 + LockXOffsetFromCenter, y - cursor.CursorVisual.ActualHeight / 2 + LockYOffsetFromCenter);

            // Finds the current position
            Point cursorPoint = new Point(e.X - cursor.CursorVisual.ActualWidth / 2, e.Y - cursor.CursorVisual.ActualHeight / 2);

            // Guide the cursor to the target position
            DoubleAnimation moveLeft = new DoubleAnimation(lockPoint.X, cursorPoint.X, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveLeft, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Canvas.LeftProperty));
            DoubleAnimation moveTop = new DoubleAnimation(lockPoint.Y, cursorPoint.Y, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveTop, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveTop, new PropertyPath(Canvas.TopProperty));

            move = new Storyboard();
            move.Children.Add(moveTop);
            move.Children.Add(moveLeft);

            move.Completed += delegate
            {
                this.RaiseEvent(new KinectCursorEventArgs(KinectCursorLockEvent, new Point(x + LockXOffsetFromCenter, y + LockYOffsetFromCenter), e.Z) { Cursor = e.Cursor });
            };

            if (move != null)
                move.Stop(e.Cursor);
            move.Begin(cursor, false);
            base.OnKinectCursorEnter(sender, e);
        }

        protected override void OnKinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            if (this.Opacity == 0)
                return;
            base.OnKinectCursorLeave(sender, e);
            if (!_isLockedOn)
                return;

            //if (move != null)
            //    move.Stop(e.Cursor);

            e.Cursor.UpdateCursor(new Point(e.X, e.Y), false);

            // button position again
            var rootVisual = FindAncestor<Window>(this);
            var point = this.TransformToAncestor(rootVisual).Transform(new Point(0, 0));
            var x = point.X + this.ActualWidth / 2;
            var y = point.Y + this.ActualHeight / 2;

            var cursor = e.Cursor;

            // Lock on target
            Point lockPoint = new Point(x - cursor.CursorVisual.ActualWidth / 2 + LockXOffsetFromCenter, y - cursor.CursorVisual.ActualHeight / 2 + LockYOffsetFromCenter);

            // Find current position
            Point cursorPoint = new Point(e.X - cursor.CursorVisual.ActualWidth / 2, e.Y - cursor.CursorVisual.ActualHeight / 2);

            DoubleAnimation moveLeft = new DoubleAnimation(lockPoint.X, cursorPoint.X, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveLeft, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Canvas.LeftProperty));
            DoubleAnimation moveTop = new DoubleAnimation(lockPoint.Y, cursorPoint.Y, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveTop, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveTop, new PropertyPath(Canvas.TopProperty));

            move = new Storyboard();
            move.Children.Add(moveTop);
            move.Children.Add(moveLeft);

            move.Completed += delegate
            {
                move.Stop(cursor);
                cursor.UpdateCursor(new Point(e.X, e.Y), false);
                this.RaiseEvent(new KinectCursorEventArgs(KinectCursorUnlockEvent, new Point(e.X, e.Y), e.Z) { Cursor = e.Cursor });
            };
            move.Begin(cursor, true);
        }

        protected override void OnKinectCursorMove(object sender, KinectCursorEventArgs e)
        {
            _lastPointDetected = e;
        }

        protected override void OnKinectCursorDeactivate(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new KinectCursorEventArgs(KinectCursorUnlockEvent, new Point(_lastPointDetected.X, _lastPointDetected.Y), _lastPointDetected.Z) { Cursor = _lastPointDetected.Cursor });
        }

        protected override void OnKinectCursorActivate(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new KinectCursorEventArgs(KinectCursorEnterEvent, new Point(_lastPointDetected.X, _lastPointDetected.Y), _lastPointDetected.Z) { Cursor = _lastPointDetected.Cursor });
        }

        private T FindAncestor<T>(DependencyObject _dependencyObject)
            where T : class
        {
            DependencyObject target = _dependencyObject;
            do
            {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));

            return target as T;
        }
        #endregion Methods and Overrides
    }
}
