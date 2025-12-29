using System;

namespace SOTFEdit.Model;

public class Area(string name, int areaMask, int graphMask)
{
    public string Name { get; } = name;
    public int AreaMask { get; } = areaMask;
    public int GraphMask { get; } = graphMask;

    public bool IsUnderground()
    {
        return AreaMask != 0;
    }

    private bool Equals(Area other)
    {
        return AreaMask == other.AreaMask && GraphMask == other.GraphMask;
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

        return obj.GetType() == GetType() && Equals((Area)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AreaMask, GraphMask);
    }

    public bool IsSurface()
    {
        return !IsUnderground();
    }
}