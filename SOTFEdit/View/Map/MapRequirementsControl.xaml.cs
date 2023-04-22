using System.Windows;
using SOTFEdit.Model.Map;

namespace SOTFEdit.View.Map;

public partial class MapRequirementsControl
{
    public static readonly DependencyProperty PoiProperty = DependencyProperty.Register(
        nameof(Poi), typeof(InformationalPoi), typeof(MapRequirementsControl),
        new PropertyMetadata(default(InformationalPoi)));

    public MapRequirementsControl()
    {
        InitializeComponent();
    }

    public InformationalPoi Poi
    {
        get => (InformationalPoi)GetValue(PoiProperty);
        set => SetValue(PoiProperty, value);
    }
}