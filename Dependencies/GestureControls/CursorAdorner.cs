
using System.Windows.Media;

namespace GestureControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Documents;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows.Media.Animation;
    using System.Diagnostics;

    public class CursorAdorner : Adorner
    {
        #region Member Variables
        private readonly UIElement _adorningElement;
        private VisualCollection _visualChildren;
        private Canvas _cursorCanvas;
        protected FrameworkElement _cursor;
        private bool _isVisible;
        private bool _isOverridden;
        Storyboard _gradientStopAnimationStoryboard;

        // Default Cursor Colors... come here to change them
        readonly static Color _backColor = Colors.Blue;
        readonly static Color _foreColor = Colors.Black;
        readonly static int _strokethickness = 7;
        readonly static int _radius = 30;
        #endregion Member Variables


        #region Constructors
        public CursorAdorner(FrameworkElement adorningElement)
            : base(adorningElement)
        {
            if (adorningElement == null)
                throw new ArgumentNullException("adorningElement");
            this._adorningElement = adorningElement;
            CreateCursorAdorner();
            this.IsHitTestVisible = false;
            //_adorningElement.Visibility = System.Windows.Visibility.Collapsed;
        }

        public CursorAdorner(FrameworkElement adorningElement, FrameworkElement innerCursor)
            : base(adorningElement)
        {
            if (adorningElement == null)
                throw new ArgumentNullException("Adorning Element Parameter Empty");
            this._adorningElement = adorningElement;
            CreateCursorAdorner(innerCursor);
            this.IsHitTestVisible = false;
            //_adorningElement.Visibility = System.Windows.Visibility.Collapsed;
        }

        public FrameworkElement CursorVisual
        {
            get { return _cursor; }
        }

        public void CreateCursorAdorner()
        {
            var innerCursor = CreateCursor();
            CreateCursorAdorner(innerCursor);
        }

        protected FrameworkElement CreateCursor()
        {
            var brush = new LinearGradientBrush();
            brush.EndPoint = new Point(0, 1);
            brush.StartPoint = new Point(0, 0);
            brush.GradientStops.Add(new GradientStop(_backColor, 1));
            brush.GradientStops.Add(new GradientStop(_foreColor, 1));
            var stroke = new LinearGradientBrush();
            stroke.EndPoint = new Point(0, 1);
            stroke.StartPoint = new Point(0, 0);
            stroke.GradientStops.Add(new GradientStop(_foreColor, 1));
            stroke.GradientStops.Add(new GradientStop(_backColor, 1));
            var cursor = new Ellipse()
            {
                Width = _radius,
                Height = _radius,
                Fill = brush,
                Stroke = stroke,
                StrokeThickness = _strokethickness
            };
            return cursor;
        }

        public void CreateCursorAdorner(FrameworkElement innerCursor)
        {
            _visualChildren = new VisualCollection(this);
            _cursorCanvas = new Canvas();
            _cursor = innerCursor;
            _cursorCanvas.Children.Add(_cursor);
            _visualChildren.Add(this._cursorCanvas);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_adorningElement);
            layer.Add(this);
        }
        #endregion Constructors


        #region Overrides
        protected override int VisualChildrenCount
        {
            get
            {
                return _visualChildren.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this._cursorCanvas.Measure(constraint);
            return this._cursorCanvas.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._cursorCanvas.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public void SetVisibility(bool isVisible)
        {
            if (!isVisible)
                _cursorCanvas.Visibility = Visibility.Collapsed;
            if (isVisible)
                _cursorCanvas.Visibility = Visibility.Visible;
            this._isVisible = isVisible;
        }
        #endregion Overrides


        #region Methods and Animations
        public void UpdateCursor(Point position, bool isOverride)
        {
            _isOverridden = isOverride;
            _cursor.SetValue(Canvas.LeftProperty, position.X - (_cursor.ActualWidth / 2));
            _cursor.SetValue(Canvas.TopProperty, position.Y - (_cursor.ActualHeight / 2));
        }

        public void UpdateCursor(Point position)
        {
            if (_isOverridden)
                return;

            _cursor.SetValue(Canvas.LeftProperty, position.X - (_cursor.ActualWidth / 2));
            _cursor.SetValue(Canvas.TopProperty, position.Y - (_cursor.ActualHeight / 2));
        }

        public virtual void AnimateCursor(double milliSeconds)
        {
            CreateGradientStopAnimation(milliSeconds);
            if (_gradientStopAnimationStoryboard != null)
                _gradientStopAnimationStoryboard.Begin(this, true);
        }

        public virtual void StopCursorAnimation()
        {
            if (_gradientStopAnimationStoryboard != null)
                _gradientStopAnimationStoryboard.Stop(this);
        }

        public virtual void CreateGradientStopAnimation(double milliSeconds)
        {
            NameScope.SetNameScope(this, new NameScope());
            var cursor = _cursor as Shape;
            if (cursor == null) return;
            var brush = cursor.Fill as LinearGradientBrush;
            var stop1 = brush.GradientStops[0];
            var stop2 = brush.GradientStops[1];
            this.RegisterName("GradientStop1", stop1);
            this.RegisterName("GradientStop2", stop2);

            DoubleAnimation offsetAnimation1 = new DoubleAnimation();
            offsetAnimation1.From = 1.0;
            offsetAnimation1.To = 0.0;
            offsetAnimation1.Duration = TimeSpan.FromMilliseconds(milliSeconds);

            Storyboard.SetTargetName(offsetAnimation1, "GradientStop1");
            Storyboard.SetTargetProperty(offsetAnimation1, new PropertyPath(GradientStop.OffsetProperty));


            DoubleAnimation offsetAnimation2 = new DoubleAnimation();
            offsetAnimation2.From = 1.0;
            offsetAnimation2.To = 0.0;
            offsetAnimation2.Duration = TimeSpan.FromMilliseconds(milliSeconds);

            Storyboard.SetTargetName(offsetAnimation2, "GradientStop2");
            Storyboard.SetTargetProperty(offsetAnimation2, new PropertyPath(GradientStop.OffsetProperty));

            _gradientStopAnimationStoryboard = new Storyboard();
            _gradientStopAnimationStoryboard.Children.Add(offsetAnimation1);
            _gradientStopAnimationStoryboard.Children.Add(offsetAnimation2);
            _gradientStopAnimationStoryboard.Completed += delegate { _gradientStopAnimationStoryboard.Stop(this); };
        }
        #endregion Methods and Animations
    }
}
