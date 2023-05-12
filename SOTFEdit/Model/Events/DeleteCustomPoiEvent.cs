namespace SOTFEdit.Model.Events;

public class DeleteCustomPoiEvent
{
    public DeleteCustomPoiEvent(int id)
    {
        Id = id;
    }

    public int Id { get; }
}