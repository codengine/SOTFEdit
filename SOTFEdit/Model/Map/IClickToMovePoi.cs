namespace SOTFEdit.Model.Map;

public interface IClickToMovePoi
{
    public bool IsMoveRequested { get; set; }
    public void AcceptNewPos(Position newPosition);
}