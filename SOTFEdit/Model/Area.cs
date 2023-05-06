using System;

namespace SOTFEdit.Model;

public class Area
{
    public Area(string name, int areaMask, int graphMask)
    {
        Name = name;
        AreaMask = areaMask;
        GraphMask = graphMask;
    }

    public string Name { get; }
    public int AreaMask { get; }
    public int GraphMask { get; }

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

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Area)obj);
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