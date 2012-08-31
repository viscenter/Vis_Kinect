using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using GestureControls.Input;
using System.Windows;
using System.Diagnostics;


namespace GestureControls.Controls
{
    public class KinectButton : Button
    {
        #region Events
        public static readonly RoutedEvent KinectCursorEnterEvent = KinectInput.KinectCursorEnterEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorLeaveEvent = KinectInput.KinectCursorLeaveEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorMoveEvent = KinectInput.KinectCursorMoveEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorActivateEvent = KinectInput.KinectCursorActivateEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorDeactivateEvent = KinectInput.KinectCursorDeactivateEvent.AddOwner(typeof(KinectButton));

        public event KinectCursorEventHandler KinectCursorEnter
        {
            add { base.AddHandler(KinectCursorEnterEvent, value); }
            remove { base.RemoveHandler(KinectCursorEnterEvent, value); }
        }

        public event KinectCursorEventHandler KinectCursorLeave
        {
            add { base.AddHandler(KinectCursorLeaveEvent, value); }
            remove { base.RemoveHandler(KinectCursorLeaveEvent, value); }
        }

        public event KinectCursorEventHandler KinectCursorMove
        {
            add { base.AddHandler(KinectCursorMoveEvent, value); }
            remove { base.RemoveHandler(KinectCursorMoveEvent, value); }
        }

        public event RoutedEventHandler KinectCursorActivate
        {
            add { base.AddHandler(KinectCursorActivateEvent, value); }
            remove { base.RemoveHandler(KinectCursorActivateEvent, value); }
        }

        public event RoutedEventHandler KinectCursorDeactivate
        {
            add { base.AddHandler(KinectCursorDeactivateEvent, value); }
            remove { base.RemoveHandler(KinectCursorDeactivateEvent, value); }
        }
        #endregion Events


        #region Constructor
        public KinectButton()
        {
            // autogeneration of manager
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                KinectCursorManager.Create(Application.Current.MainWindow);

            this.KinectCursorEnter += new KinectCursorEventHandler(OnKinectCursorEnter);
            this.KinectCursorLeave += new KinectCursorEventHandler(OnKinectCursorLeave);
            this.KinectCursorMove += new KinectCursorEventHandler(OnKinectCursorMove);
            this.KinectCursorActivate += new RoutedEventHandler(OnKinectCursorActivate);
            this.KinectCursorDeactivate += new RoutedEventHandler(OnKinectCursorDeactivate);
        }

        protected virtual void OnKinectCursorEnter(Object sender, KinectCursorEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        protected virtual void OnKinectCursorLeave(Object sender, KinectCursorEventArgs e) { }
        protected virtual void OnKinectCursorMove(Object sender, KinectCursorEventArgs e) { }
        protected virtual void OnKinectCursorActivate(Object sender, RoutedEventArgs e) { }
        protected virtual void OnKinectCursorDeactivate(Object sender, RoutedEventArgs e) { }
        #endregion Constructors
    }
}
