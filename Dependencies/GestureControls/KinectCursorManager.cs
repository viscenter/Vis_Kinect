using System.Collections.Generic;
using System.Windows;
using System;
using GestureControls.Input;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;

namespace GestureControls
{
    public class KinectCursorManager
    {
        #region Member Variables
        private KinectSensor _kinectSensor;
        private CursorAdorner _cursorAdorner;
        private readonly Window _window;
        private UIElement _lastElementOver;
        private bool _isHandTrackingActivated;
        private static bool _isInitialized;
        private static KinectCursorManager _instance;
        private List<GesturePoint> _gesturePoints;
        private bool _gesturePointTrackingEnabled;
        private double _swipeLength, _swipeDeviation;
        private int _swipeTime;
        private bool _hasHandThreshold = true;
        
        private double _xOutOfBoundsLength;
        private static double _initialSwipeX;

        // Screen dimensions and scale value
        //public float posX;
        //public float poY;
        //private int HEIGHT = 1024;
        //private int WIDTH = 1280;
        //private float SCALE = 0.3f;

        public event KinectCursorEventHandler SwipeDeteted;

        public event KinectCursorEventHandler SwipeOutOfBoundsDetected;
        #endregion Member Variables


        #region Overloads
        public static void Create(Window window)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window);
                _isInitialized = true;
            }
        }

        public static void Create(Window window, FrameworkElement cursor)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window, cursor);
                _isInitialized = true;
            }
        }

        public static void Create(Window window, KinectSensor sensor)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window, sensor);
                _isInitialized = true;
            }
        }

        public static void Create(Window window, KinectSensor sensor, FrameworkElement cursor)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window, sensor, cursor);
                _isInitialized = true;
            }
        }

        public static KinectCursorManager Instance
        {
            get { return _instance; }
        }

        private KinectCursorManager(Window window)
            : this(window, KinectSensor.KinectSensors[0]) { }

        private KinectCursorManager(Window window, FrameworkElement cursor)
            : this(window, KinectSensor.KinectSensors[0], cursor) { }

        private KinectCursorManager(Window window, KinectSensor sensor)
            : this(window, sensor, null) { }

        private KinectCursorManager(Window window, KinectSensor sensor, FrameworkElement cursor)
        {
            this._window = window;
            this._gesturePoints = new List<GesturePoint>();

            if (KinectSensor.KinectSensors.Count > 0)
            {
                _window.Unloaded += delegate
                {
                    if (this._kinectSensor.SkeletonStream.IsEnabled)
                        this._kinectSensor.SkeletonStream.Disable();
                    _kinectSensor.Stop();
                };

                _window.Loaded += delegate
                {
                    if (cursor == null)
                        _cursorAdorner = new CursorAdorner((FrameworkElement)window.Content);
                    else
                        _cursorAdorner = new CursorAdorner((FrameworkElement)window.Content, cursor);

                    this._kinectSensor = sensor;

                    this._kinectSensor.SkeletonFrameReady += SkeletonFrameReady;
                    this._kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
                    {
                        Correction = 0.5f,
                        JitterRadius = 0.05f,
                        MaxDeviationRadius = 0.04f,
                        Smoothing = 0.1f
                    });
                    this._kinectSensor.Start();
                };
            }
        }
        #endregion Overloads


        #region TrackElements
        private void SetHandTrackingActivated()
        {
            _cursorAdorner.SetVisibility(true);
            if (_lastElementOver != null && _isHandTrackingActivated == false)
            {
                _lastElementOver.RaiseEvent(new RoutedEventArgs(KinectInput.KinectCursorActivateEvent));
            };
            _isHandTrackingActivated = true;
        }

        private void SetHandTrackingDeactivated()
        {
            _cursorAdorner.SetVisibility(false);
            if (_lastElementOver != null && _isHandTrackingActivated == true)
            {
                _lastElementOver.RaiseEvent(new RoutedEventArgs(KinectInput.KinectCursorDeactivateEvent));
            };
            _isHandTrackingActivated = false;
        }

        private void HandleCursorEvents(Point point, double z, Joint joint)
        {
            UIElement element = GetElementAtScreenPoint(point, _window);
            if (element != null)
            {
                element.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorMoveEvent, point, z) { Cursor = _cursorAdorner });
                if (element != _lastElementOver)
                {
                    if (_lastElementOver != null)
                    {
                        _lastElementOver.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorLeaveEvent, point, z) { Cursor = _cursorAdorner });
                    }

                    element.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorEnterEvent, point, z) { Cursor = _cursorAdorner });
                }
            }

            _lastElementOver = element;
        }
        #endregion TrackElements


        #region SkeletonFrames
        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null || frame.SkeletonArrayLength == 0)
                    return;

                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                Skeleton skeleton = GetPrimarySkeleton(skeletons);

                if (skeleton == null)
                {
                    SetHandTrackingDeactivated();
                }
                else
                {
                    Joint? primaryHand = GetPrimaryHand(skeleton);
                    if (primaryHand.HasValue)
                    {
                        UpdateCursor(primaryHand.Value);
                    }
                    else
                    {
                        SetHandTrackingDeactivated();
                    }
                }
            }
        }

        private void UpdateCursor(Joint hand)
        {
            // Scaled
            //var point = _kinectSensor.MapSkeletonPointToDepth(hand.Position, _kinectSensor.DepthStream.Format);
            //Joint _NewHand = hand.ScaleTo(WIDTH, HEIGHT, SCALE, SCALE);
            //float z = point.Depth;
            //SetHandTrackingActivated();
            //Point cursorPoint = new Point(_NewHand.Position.X, _NewHand.Position.Y);
            //HandleCursorEvents(cursorPoint, z, hand);
            //_cursorAdorner.UpdateCursor(cursorPoint);


            // Not Scaled
            var point = _kinectSensor.MapSkeletonPointToDepth(hand.Position, _kinectSensor.DepthStream.Format);
            float x = point.X;
            float y = point.Y;
            float z = point.Depth;

            SetHandTrackingActivated();
            x = (float)(x * _window.ActualWidth / _kinectSensor.DepthStream.FrameWidth);
            y = (float)(y * _window.ActualHeight / _kinectSensor.DepthStream.FrameHeight);
            Point cursorPoint = new Point(x, y);
            HandleGestureTracking(x, y, z);
            HandleCursorEvents(cursorPoint, z, hand);
            _cursorAdorner.UpdateCursor(cursorPoint);
        }

        #endregion SkeletonFrames


        #region HelperMethods
        private static UIElement GetElementAtScreenPoint(Point point, Window window)
        {
            if (!window.IsVisible)
            {
                return null;
            }

            Point windowPoint = window.PointFromScreen(point);

            IInputElement element = window.InputHitTest(windowPoint);
            if (element is UIElement)
                return (UIElement)element;
            else
                return null;
        }


        private static Skeleton GetPrimarySkeleton(IEnumerable<Skeleton> skeletons)
        {
            Skeleton primarySkeleton = null;
            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (primarySkeleton == null)
                {
                    primarySkeleton = skeleton;
                }
                else if (primarySkeleton.Position.Z > skeleton.Position.Z)
                {
                    primarySkeleton = skeleton;
                }
            }
            return primarySkeleton;
        }

        private static Joint? GetPrimaryHand(Skeleton skeleton)
        {
            Joint leftHand = skeleton.Joints[JointType.HandLeft];
            Joint rightHand = skeleton.Joints[JointType.HandRight];

            if (rightHand.TrackingState == JointTrackingState.Tracked)
            {
                if (leftHand.TrackingState != JointTrackingState.Tracked)
                    return rightHand;
                else if (leftHand.Position.Z > rightHand.Position.Z)
                    return rightHand;
                else return leftHand;
            }

            if (leftHand.TrackingState == JointTrackingState.Tracked)
                return leftHand;
            else return null;
        }
        #endregion HelperMethods


        #region GestureTracking
        public bool HasHandThreshold
        {
            get { return _hasHandThreshold; }
            set { _hasHandThreshold = value; }
        }

        private void HandleGestureTracking(float x, float y, float z)
        {
            if (!_gesturePointTrackingEnabled)
                return;
            if (_xOutOfBoundsLength != 0 && _initialSwipeX == 0)
            {
                _initialSwipeX = x;
            }

            GesturePoint newPoint = new GesturePoint() { X = x, Y = y, Z = z, T = DateTime.Now };
            GesturePoints.Add(newPoint);

            GesturePoint startPoint = GesturePoints[0];
            var point = new Point(x, y);

            if (Math.Abs(newPoint.Y - startPoint.Y) > _swipeDeviation)
            {
                // Debug.WriteLine("Y is out of bounds");
                if (SwipeOutOfBoundsDetected != null)
                    SwipeOutOfBoundsDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = _cursorAdorner });
                ResetGesturePoint(GesturePoints.Count);
                return;
            }

            if ((newPoint.T - startPoint.T).Milliseconds > _swipeTime) // this checks the time
            {
                GesturePoints.RemoveAt(0);
                startPoint = GesturePoints[0];
            }

            if ((_swipeLength < 0 && newPoint.X - startPoint.X < _swipeLength) ||
                (_swipeLength > 0 && newPoint.X - startPoint.X > _swipeLength))
            {
                GesturePoints.Clear();

                // throw local event
                if (SwipeDeteted != null)
                    SwipeDeteted(this, new KinectCursorEventArgs(point) { Z = z, Cursor = _cursorAdorner });
                return;
            }

            if (_xOutOfBoundsLength != 0 &&
                ((_xOutOfBoundsLength < 0 && newPoint.X - _initialSwipeX < _xOutOfBoundsLength) ||
                (_xOutOfBoundsLength > 0 && newPoint.X - _initialSwipeX > _xOutOfBoundsLength)))
            {
                if (SwipeOutOfBoundsDetected != null)
                    SwipeOutOfBoundsDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = _cursorAdorner });
            }
        }

        public IList<GesturePoint> GesturePoints
        {
            get { return _gesturePoints; }
        }

        public bool GesturePointTrackingEnabled
        {
            get { return _gesturePointTrackingEnabled; }
        }

        public void GesturePointTrackingInitialize(double swipeLength, double swipeDeviation, int swipeTime, double xOutOfBounds)
        {
            _swipeLength = swipeLength;
            _swipeDeviation = swipeDeviation;
            _swipeTime = swipeTime;
            _xOutOfBoundsLength = xOutOfBounds;
        }

        public void GesturePointTrackingStart()
        {
            if (_swipeLength == 0 || _swipeDeviation == 0 || _swipeTime == 0)
                throw (new InvalidOperationException("Swipe detection not initialized"));
            _gesturePointTrackingEnabled = true;
        }

        public void GesturePointTrackingStop()
        {
            _xOutOfBoundsLength = 0;
            _gesturePointTrackingEnabled = false;
            _gesturePoints.Clear();
        }

        public void ResetGesturePoint(GesturePoint point)
        {
            bool startRemoving = false;
            for (int i = GesturePoints.Count; i >= 0; i--)
            {
                if (startRemoving)
                    GesturePoints.RemoveAt(i);
                else
                    if (GesturePoints[i].Equals(point))
                        startRemoving = true;
            }
        }

        public void ResetGesturePoint(int point)
        {
            if (point < 1)
                return;
            for (int i = point - 1; i >= 0; i--)
            {
                GesturePoints.RemoveAt(i);
            }
        }
        #endregion GestureTracking

    }
}
