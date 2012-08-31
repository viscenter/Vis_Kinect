

namespace GestureControls.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    
    public delegate void KinectCursorEventHandler(object sender, KinectCursorEventArgs e);

    public static class KinectInput
    {
        #region KinectCursorEnter
        public static readonly RoutedEvent KinectCursorEnterEvent =
            EventManager.RegisterRoutedEvent("KinectCursorEnter", RoutingStrategy.Bubble,
                                            typeof(KinectCursorEventHandler), typeof(KinectInput));

        public static void AddKinectCursorEnterandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).AddHandler(KinectCursorEnterEvent, handler); }

        public static void RemoveKinectCursorEnterHandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).RemoveHandler(KinectCursorEnterEvent, handler); }
        #endregion KinectCursorEnter


        #region KinectCursorLeave
        public static readonly RoutedEvent KinectCursorLeaveEvent =
            EventManager.RegisterRoutedEvent("KinectCursorLeave", RoutingStrategy.Bubble,
                                            typeof(KinectCursorEventHandler), typeof(KinectInput));

        public static void AddKinectCursorLeaveHandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).AddHandler(KinectCursorLeaveEvent, handler); }

        public static void RemoveKinectCursorLeaveHandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).RemoveHandler(KinectCursorLeaveEvent, handler); }
        #endregion KinectCursorLeave


        #region KinectCursorActivate
        public static readonly RoutedEvent KinectCursorActivateEvent =
            EventManager.RegisterRoutedEvent("KinectCursorActivate", RoutingStrategy.Bubble,
                                            typeof(RoutedEventHandler), typeof(KinectInput));

        public static void AddKinectCursorActivateHandler(DependencyObject o, RoutedEventHandler handler)
        { ((UIElement)o).AddHandler(KinectCursorActivateEvent, handler); }

        public static void RemoveKinectCursorActivateHandler(DependencyObject o, RoutedEventHandler handler)
        { ((UIElement)o).RemoveHandler(KinectCursorActivateEvent, handler); }
        #endregion KinectCursorActivate


        #region KinectCursorDeactivate
        public static readonly RoutedEvent KinectCursorDeactivateEvent =
            EventManager.RegisterRoutedEvent("KinectCursorDeactivate", RoutingStrategy.Bubble,
                                            typeof(RoutedEventHandler), typeof(KinectInput));

        public static void AddKinectCursorDeactivateHandler(DependencyObject o, RoutedEventHandler handler)
        { ((UIElement)o).AddHandler(KinectCursorDeactivateEvent, handler); }

        public static void RemoveKinectCursorDeactivateHandler(DependencyObject o, RoutedEventHandler handler)
        { ((UIElement)o).RemoveHandler(KinectCursorDeactivateEvent, handler); }
        #endregion KinectCursorDeactivate


        #region KinectCursorMove
        public static readonly RoutedEvent KinectCursorMoveEvent =
            EventManager.RegisterRoutedEvent("KinectCursorMove", RoutingStrategy.Bubble,
                                            typeof(KinectCursorEventHandler), typeof(KinectInput));

        public static void AddKinectCursorMoveHandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).AddHandler(KinectCursorMoveEvent, handler); }

        public static void RemoveKinectCursorMoveHandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).RemoveHandler(KinectCursorMoveEvent, handler); }
        #endregion KinectCursorMove


        #region KinectCursorLock
        public static readonly RoutedEvent KinectCursorLockEvent =
            EventManager.RegisterRoutedEvent("KinectCursorLock", RoutingStrategy.Bubble,
                                            typeof(KinectCursorEventHandler), typeof(KinectInput));

        public static void AddKinectCursorLockHandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).AddHandler(KinectCursorLockEvent, handler); }
        #endregion KinectCursorLock


        #region KinectCursorUnlock
        public static readonly RoutedEvent KinectCursorUnlockEvent =
            EventManager.RegisterRoutedEvent("KinectCursorUnlock", RoutingStrategy.Bubble,
                                            typeof(KinectCursorEventHandler), typeof(KinectInput));

        public static void AddKinectCursorUnlockHandler(DependencyObject o, KinectCursorEventHandler handler)
        { ((UIElement)o).AddHandler(KinectCursorUnlockEvent, handler); }
        #endregion KinectCursorUnlock
    }
}