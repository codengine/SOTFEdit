namespace SOTFEdit.Model.Events;

public class SelectedPoiChangedEvent
{
    public SelectedPoiChangedEvent(bool isSelected)
    {
        IsSelected = isSelected;
    }

    public bool IsSelected { get; }
}