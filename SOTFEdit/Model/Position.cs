using System;
using Newtonsoft.Json;

namespace SOTFEdit.Model;

public class Position
{
    private readonly float _y;

    public Position(float x, float y, float z)
    {
        X = x;
        _y = y;
        Z = z;
    }

    private float YOffset { get; set; }

    [JsonProperty("x")] public float X { get; init; }

    [JsonProperty("y")] public float Y => _y + YOffset;

    [JsonProperty("z")] public float Z { get; init; }

    [JsonIgnore] public string PrintableShort => $"X: {X:F2}, Y: {Y:F2}, Z: {Z:F2}";

    [JsonIgnore] public string Printable => $"X: {X}, Y: {Y}, Z: {Z}";

    public Position WithoutOffset()
    {
        return new Position(X, _y, Z);
    }

    public Position WithYOffset(int yOffset)
    {
        var newPos = WithoutOffset();
        newPos.YOffset = yOffset;
        return newPos;
    }

    private bool Equals(Position other)
    {
        return _y.Equals(other._y) && X.Equals(other.X) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Position)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_y, X, Z);
    }
}