using System.Windows.Media;

namespace SOTFEdit.Infrastructure;

public class ThemeData
{
    public ThemeData(string name, Brush colorBrush)
    {
        Name = name;
        ColorBrush = colorBrush;
    }

    public string Name { get; }

    public Brush ColorBrush { get; }

    public override string ToString()
    {
        return Name;
    }
}