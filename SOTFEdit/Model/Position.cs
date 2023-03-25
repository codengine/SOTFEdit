namespace SOTFEdit.Model;

public record Position(float X, float Y, float Z)
{
    public Position Copy()
    {
        return new Position(X, Y, Z);
    }
}