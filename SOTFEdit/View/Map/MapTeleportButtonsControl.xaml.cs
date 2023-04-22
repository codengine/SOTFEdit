using System.Windows;
using SOTFEdit.Model.Map;

namespace SOTFEdit.View.Map;

public partial class MapTeleportButtonsControl
{
    public static readonly DependencyProperty PoiProperty = DependencyProperty.Register(
        nameof(Poi), typeof(BasePoi), typeof(MapTeleportButtonsControl), new PropertyMetadata(default(BasePoi)));

    public MapTeleportButtonsControl()
    {
        InitializeComponent();
    }

    public BasePoi Poi
    {
        get => (BasePoi)GetValue(PoiProperty);
        set => SetValue(PoiProperty, value);
    }
}