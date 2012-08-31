using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GestureControls.Input;
using System.Diagnostics;

namespace GestureControls.Controls
{
    public class MagneticSlide : MagnetButton
    {
        #region MemberVariables
        private bool _isLookingForSwipes;
        #endregion MemberVariables


        #region Constructor and Events
        public MagneticSlide()
        {
            base._isLockedOn = IsMagnetOn;
            base._timerEnabled = false;
        }

        public event RoutedEventHandler SwipeOutOfBounds
        {
            add { AddHandler(SwipeOutOfBoundsEvent, value); }
            remove { RemoveHandler(SwipeOutOfBoundsEvent, value); }
        }

        public bool IsMagnetOn
        {
            get { return (bool)GetValue(IsMagnetOnProperty); }
            set { SetValue(IsMagnetOnProperty, value); }
        }

        public double SwipeLength
        {
            get { return (double)GetValue(SwipeLengthPropery); }
            set { SetValue(SwipeLengthPropery, value); }
        }

        public double MaxDeviation
        {
            get { return (double)GetValue(MaxDeviationProperty); }
            set { SetValue(MaxDeviationProperty, value); }
        }

        public double XOutOfBoundsLength
        {
            get { return (double)GetValue(XOutOfBoundsLengthProperty); }
            set { SetValue(XOutOfBoundsLengthProperty, value); }
        }

        public int MaxSwipeTime
        {
            get { return (int)GetValue(MaxSwipeTimeProperty); }
            set { SetValue(MaxSwipeTimeProperty, value); }
        }
        #endregion Constructor and Events


        #region DependencyProperties
        public static readonly RoutedEvent SwipeOutOfBoundsEvent =
            EventManager.RegisterRoutedEvent("SwipeOutOfBounds", RoutingStrategy.Bubble, typeof(KinectCursorEventHandler), typeof(KinectInput));

        public static readonly DependencyProperty IsMagnetOnProperty =
            DependencyProperty.Register("IsMagnetOn", typeof(Boolean), typeof(MagneticSlide), new UIPropertyMetadata(true));

        public static readonly DependencyProperty SwipeLengthPropery =
            DependencyProperty.Register("SwipeLength", typeof(double), typeof(MagneticSlide), new UIPropertyMetadata(-500d));

        public static readonly DependencyProperty MaxDeviationProperty =
            DependencyProperty.Register("MaxDeviation", typeof(double), typeof(MagneticSlide), new UIPropertyMetadata(100d));

        public static readonly DependencyProperty XOutOfBoundsLengthProperty =
            DependencyProperty.Register("XOutOfBoundsLength", typeof(double), typeof(MagneticSlide), new UIPropertyMetadata(-700d));

        public static readonly DependencyProperty MaxSwipeTimeProperty =
            DependencyProperty.Register("MaxSwipeTime", typeof(int), typeof(MagneticSlide), new UIPropertyMetadata(300));
        #endregion DependencyProperties


        #region Overrides and Methods
        private void InitializeSwipe()
        {
            if (_isLookingForSwipes)
                return;

            _isLookingForSwipes = true;
            var KnctManager = KinectCursorManager.Instance;
            KnctManager.GesturePointTrackingInitialize(SwipeLength, MaxDeviation, MaxSwipeTime, XOutOfBoundsLength);
            KnctManager.SwipeDeteted += KnctManager_SwipeDetected;
            KnctManager.SwipeOutOfBoundsDetected += KnctManager_SwipeOutOfBoundsDetected;
            KinectCursorManager.Instance.GesturePointTrackingStart();
        }

        private void DeInitializeSwipe()
        {
            _isLookingForSwipes = false;
            var KnctManager = KinectCursorManager.Instance;
            KnctManager.GesturePointTrackingStop();
            KnctManager.SwipeDeteted -= KnctManager_SwipeDetected;
            KnctManager.SwipeOutOfBoundsDetected -= KnctManager_SwipeOutOfBoundsDetected;
        }

        void KnctManager_SwipeOutOfBoundsDetected(object sender, KinectCursorEventArgs e)
        {
            DeInitializeSwipe();
            RaiseEvent(new KinectCursorEventArgs(SwipeOutOfBoundsEvent));
        }

        void KnctManager_SwipeDetected(object sender, KinectCursorEventArgs e)
        {
            InitializeSwipe();
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            InitializeSwipe();
            base.OnKinectCursorEnter(sender, e);
        }
        #endregion Overrides and Methods
    }
}
