using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ZoomControl;

/// <summary>
///     https://github.com/andypelzer/GraphSharp/blob/master/Graph%23.Controls
///     https://wpfextensions.codeplex.com/
/// </summary>
[TemplatePart(Name = PartPresenter, Type = typeof(ZoomContentPresenter))]
public class ZoomControl : ContentControl
{
    private const string PartPresenter = "PART_Presenter";

    public static readonly DependencyProperty AnimationLengthProperty =
        DependencyProperty.Register(nameof(AnimationLength), typeof(TimeSpan), typeof(ZoomControl),
            new UIPropertyMetadata(TimeSpan.FromMilliseconds(500)));

    public static readonly DependencyProperty MaxZoomProperty =
        DependencyProperty.Register(nameof(MaxZoom), typeof(double), typeof(ZoomControl),
            new UIPropertyMetadata(100.0));

    public static readonly DependencyProperty MinZoomProperty =
        DependencyProperty.Register(nameof(MinZoom), typeof(double), typeof(ZoomControl), new UIPropertyMetadata(0.01));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(ZoomControlModes), typeof(ZoomControl),
            new UIPropertyMetadata(ZoomControlModes.Custom, ModePropertyChanged));

    public static readonly DependencyProperty ModifierModeProperty =
        DependencyProperty.Register(nameof(ModifierMode), typeof(ZoomViewModifierMode), typeof(ZoomControl),
            new UIPropertyMetadata(ZoomViewModifierMode.None));

    public static readonly DependencyProperty TranslateXProperty =
        DependencyProperty.Register(nameof(TranslateX), typeof(double), typeof(ZoomControl),
            new UIPropertyMetadata(0.0, TranslateXPropertyChanged, TranslateXCoerce));

    public static readonly DependencyProperty TranslateYProperty =
        DependencyProperty.Register(nameof(TranslateY), typeof(double), typeof(ZoomControl),
            new UIPropertyMetadata(0.0, TranslateYPropertyChanged, TranslateYCoerce));

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(ZoomControl),
            new UIPropertyMetadata(1.0, ZoomPropertyChanged));

    private bool _isZooming;
    private bool _mouseCaptured;

    private Point _mouseDownPosition;
    private ZoomContentPresenter? _presenter;

    /// <summary>Applied to the presenter.</summary>
    private ScaleTransform? _scaleTransform;

    private Vector _startTranslate;
    private TransformGroup _transformGroup;

    /// <summary>Applied to the scrollviewer.</summary>
    private TranslateTransform? _translateTransform;

    private int _zoomAnimCount;

    static ZoomControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl),
            new FrameworkPropertyMetadata(typeof(ZoomControl)));
    }

    public ZoomControl()
    {
        PreviewMouseWheel += ZoomControlMouseWheel;
        PreviewMouseDown += (_, e) => OnMouseDown(e, true);
        MouseDown += (_, e) => OnMouseDown(e, false);
        MouseUp += ZoomControlMouseUp;
    }

    public bool IsPanning { get; private set; }

    public double TranslateX
    {
        get => (double)GetValue(TranslateXProperty);
        set
        {
            BeginAnimation(TranslateXProperty, null);
            SetValue(TranslateXProperty, value);
        }
    }

    public double TranslateY
    {
        get => (double)GetValue(TranslateYProperty);
        set
        {
            BeginAnimation(TranslateYProperty, null);
            SetValue(TranslateYProperty, value);
        }
    }

    public TimeSpan AnimationLength
    {
        get => (TimeSpan)GetValue(AnimationLengthProperty);
        set => SetValue(AnimationLengthProperty, value);
    }

    public double MinZoom
    {
        get => (double)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public double MaxZoom
    {
        get => (double)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set
        {
            if (Math.Abs(value - (double)GetValue(ZoomProperty)) < 0.00001)
            {
                return;
            }

            BeginAnimation(ZoomProperty, null);
            SetValue(ZoomProperty, value);
        }
    }

    private ZoomContentPresenter? Presenter
    {
        get => _presenter;
        set
        {
            _presenter = value;
            if (_presenter == null)
            {
                return;
            }

            //add the ScaleTransform to the presenter
            _transformGroup = new TransformGroup();
            _scaleTransform = new ScaleTransform();
            _translateTransform = new TranslateTransform();
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);
            _presenter.RenderTransform = _transformGroup;
            _presenter.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }

    /// <summary>Gets or sets the active modifier mode.</summary>
    public ZoomViewModifierMode ModifierMode
    {
        get => (ZoomViewModifierMode)GetValue(ModifierModeProperty);
        set => SetValue(ModifierModeProperty, value);
    }

    /// <summary>Gets or sets the mode of the zoom control.</summary>
    public ZoomControlModes Mode
    {
        get => (ZoomControlModes)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    private static void ModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zc = (ZoomControl)d;
        var mode = (ZoomControlModes)e.NewValue;
        switch (mode)
        {
            case ZoomControlModes.Fill:
                zc.DoZoomToFill();
                break;
            case ZoomControlModes.Original:
                zc.DoZoomToOriginal();
                break;
            case ZoomControlModes.Custom:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static object TranslateXCoerce(DependencyObject d, object basevalue)
    {
        var zc = (ZoomControl)d;
        return zc._presenter == null ? 0.0 : (double)basevalue;
    }

    private static object TranslateYCoerce(DependencyObject d, object basevalue)
    {
        var zc = (ZoomControl)d;
        return zc._presenter == null ? 0.0 : (double)basevalue;
    }

    private void ZoomControlMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (ModifierMode != ZoomViewModifierMode.Pan)
        {
            return;
        }

        ModifierMode = ZoomViewModifierMode.None;
        PreviewMouseMove -= ZoomControlPreviewMouseMove;
        ReleaseMouseCapture();
    }

    private void ZoomControlPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (ModifierMode != ZoomViewModifierMode.Pan)
        {
            return;
        }

        if (!_mouseCaptured)
        {
            Mouse.Capture(this);
            _mouseCaptured = true;
        }

        var posDif = e.GetPosition(this) - _mouseDownPosition;

        var translate = _startTranslate + posDif;
        TranslateX = translate.X;
        TranslateY = translate.Y;
        IsPanning = posDif.LengthSquared > 0.1;
    }

    private void OnMouseDown(MouseEventArgs e, bool isPreview)
    {
        if (ModifierMode != ZoomViewModifierMode.None)
        {
            return;
        }

        switch (Keyboard.Modifiers)
        {
            case ModifierKeys.None:
                if (!isPreview)
                {
                    ModifierMode = ZoomViewModifierMode.Pan;
                }

                break;
            case ModifierKeys.Control:
                break;
            case ModifierKeys.Shift:
                ModifierMode = ZoomViewModifierMode.Pan;
                break;
            case ModifierKeys.Windows:
                break;
            default:
                return;
        }

        if (ModifierMode == ZoomViewModifierMode.None)
        {
            return;
        }

        IsPanning = false;
        _mouseDownPosition = e.GetPosition(this);
        _startTranslate = new Vector(TranslateX, TranslateY);
        PreviewMouseMove += ZoomControlPreviewMouseMove;
    }

    private static void TranslateXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zc = (ZoomControl)d;
        if (zc._translateTransform == null)
        {
            return;
        }

        zc._translateTransform.X = (double)e.NewValue;
        if (!zc._isZooming)
        {
            zc.Mode = ZoomControlModes.Custom;
        }
    }

    private static void TranslateYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zc = (ZoomControl)d;
        if (zc._translateTransform == null)
        {
            return;
        }

        zc._translateTransform.Y = (double)e.NewValue;
        if (!zc._isZooming)
        {
            zc.Mode = ZoomControlModes.Custom;
        }
    }

    private static void ZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zc = (ZoomControl)d;

        if (zc._scaleTransform == null)
        {
            return;
        }

        var zoom = (double)e.NewValue;
        zc._scaleTransform.ScaleX = zoom;
        zc._scaleTransform.ScaleY = zoom;
        if (!zc._isZooming)
        {
            var delta = (double)e.NewValue / (double)e.OldValue;
            zc.TranslateX *= delta;
            zc.TranslateY *= delta;
            zc.Mode = ZoomControlModes.Custom;
        }
    }

    private void ZoomControlMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        var origoPosition = new Point(ActualWidth / 2, ActualHeight / 2);
        var mousePosition = e.GetPosition(this);

        var deltaZoom = Math.Max(0.2, Math.Min(2.0, e.Delta / 300.0 + 1));
        DoZoom(deltaZoom, origoPosition, mousePosition, mousePosition);
    }

    public void ZoomToPos(float targetX, float targetY, int yOffset)
    {
        var zoom = Zoom;

        var transformX = -((targetX - ActualWidth / 2) * zoom);
        var transformY = -((targetY - ActualHeight / 2) * zoom) + yOffset;

        DoZoomAnimation(Zoom, transformX, transformY);
        Mode = ZoomControlModes.Custom;
    }

    private void DoZoom(double deltaZoom, Point origoPosition, Point startHandlePosition, Point targetHandlePosition)
    {
        var startZoom = Zoom;
        var currentZoom = startZoom * deltaZoom;
        currentZoom = Math.Max(MinZoom, Math.Min(MaxZoom, currentZoom));

        var startTranslate = new Vector(TranslateX, TranslateY);

        var v = startHandlePosition - origoPosition;
        var vTarget = targetHandlePosition - origoPosition;

        var targetPoint = (v - startTranslate) / startZoom;
        var zoomedTargetPointPos = targetPoint * currentZoom + startTranslate;
        var endTranslate = vTarget - zoomedTargetPointPos;

        var transformX = _presenter == null ? 0.0 : TranslateX + endTranslate.X;
        var transformY = _presenter == null ? 0.0 : TranslateY + endTranslate.Y;

        DoZoomAnimation(currentZoom, transformX, transformY);
        Mode = ZoomControlModes.Custom;
    }

    private void DoZoomAnimation(double targetZoom, double transformX, double transformY)
    {
        _isZooming = true;
        var duration = new Duration(AnimationLength);
        StartAnimation(TranslateXProperty, transformX, duration);
        StartAnimation(TranslateYProperty, transformY, duration);
        StartAnimation(ZoomProperty, targetZoom, duration);
    }

    private void StartAnimation(DependencyProperty dp, double toValue, Duration duration)
    {
        if (double.IsNaN(toValue) || double.IsInfinity(toValue))
        {
            if (dp == ZoomProperty)
            {
                _isZooming = false;
            }

            return;
        }

        var animation = new DoubleAnimation(toValue, duration);
        if (dp == ZoomProperty)
        {
            _zoomAnimCount++;
            animation.Completed += (_, _) =>
            {
                _zoomAnimCount--;
                if (_zoomAnimCount > 0)
                {
                    return;
                }

                var zoom = Zoom;
                BeginAnimation(ZoomProperty, null);
                SetValue(ZoomProperty, zoom);
                _isZooming = false;
            };
        }

        BeginAnimation(dp, animation, HandoffBehavior.Compose);
    }

    private void DoZoomToOriginal()
    {
        if (_presenter == null)
        {
            return;
        }

        var initialTranslate = GetInitialTranslate();
        DoZoomAnimation(1.0, initialTranslate.X, initialTranslate.Y);
    }

    private Vector GetInitialTranslate()
    {
        if (_presenter == null)
        {
            return new Vector(0.0, 0.0);
        }

        var tX = -(_presenter.ContentSize.Width - _presenter.DesiredSize.Width) / 2.0;
        var tY = -(_presenter.ContentSize.Height - _presenter.DesiredSize.Height) / 2.0;
        return new Vector(tX, tY);
    }

    public void ZoomToFill()
    {
        Mode = ZoomControlModes.Fill;
    }

    private void DoZoomToFill()
    {
        if (_presenter == null || Mode != ZoomControlModes.Fill)
        {
            return;
        }

        var deltaZoom = Math.Min(ActualWidth / _presenter.ContentSize.Width,
            ActualHeight / _presenter.ContentSize.Height);
        var initialTranslate = GetInitialTranslate();
        DoZoomAnimation(deltaZoom, initialTranslate.X * deltaZoom, initialTranslate.Y * deltaZoom);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        Presenter = GetTemplateChild(PartPresenter) as ZoomContentPresenter;
        if (Presenter != null)
        {
            Presenter.SizeChanged += (_, _) => DoZoomToFill();
            Presenter.ContentSizeChanged += (_, _) => DoZoomToFill();
        }

        ZoomToFill();
    }
}