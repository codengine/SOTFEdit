using System.Windows;
using SOTFEdit.Model.Map;

namespace SOTFEdit.View.Map;

public partial class MapItemsControl
{
    public static readonly DependencyProperty PoiProperty = DependencyProperty.Register(
        nameof(Poi), typeof(IPoiWithItems), typeof(MapItemsControl), new PropertyMetadata(default(IPoiWithItems)));

    public MapItemsControl()
    {
        InitializeComponent();
    }

    public IPoiWithItems Poi
    {
        get => (IPoiWithItems)GetValue(PoiProperty);
        set => SetValue(PoiProperty, value);
    }
}