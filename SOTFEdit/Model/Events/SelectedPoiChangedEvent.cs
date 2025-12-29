namespace SOTFEdit.Model.Events;

public class SelectedPoiChangedEvent(bool isSelected)
{
    public bool IsSelected { get; } = isSelected;
}