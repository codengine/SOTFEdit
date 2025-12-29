namespace SOTFEdit.Model.Events;

public class DeleteCustomPoiEvent(int id)
{
    public int Id { get; } = id;
}