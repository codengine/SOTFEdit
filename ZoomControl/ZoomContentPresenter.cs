using System.Windows;
using System.Windows.Controls;

namespace ZoomControl;

public class ZoomContentPresenter : ContentPresenter
{
    public delegate void ContentSizeChangedHandler(object sender, Size newSize);

    private Size _contentSize;

    public Size ContentSize
    {
        get => _contentSize;
        private set
        {
            if (value == _contentSize)
            {
                return;
            }

            _contentSize = value;
            ContentSizeChanged?.Invoke(this, _contentSize);
        }
    }

    public event ContentSizeChangedHandler? ContentSizeChanged;

    protected override Size MeasureOverride(Size constraint)
    {
        base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
        const double max = 1e9;
        var x = double.IsInfinity(constraint.Width) ? max : constraint.Width;
        var y = double.IsInfinity(constraint.Height) ? max : constraint.Height;
        return new Size(x, y);
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        if (Content is not UIElement child)
        {
            return arrangeBounds;
        }

        ContentSize = child.DesiredSize;
        child.Arrange(new Rect(child.DesiredSize));

        return arrangeBounds;
    }
}