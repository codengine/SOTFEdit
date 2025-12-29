using System.Windows.Media;

namespace SOTFEdit.Infrastructure;

public class ThemeData(string name, Brush colorBrush)
{
    public string Name { get; } = name;

    public Brush ColorBrush { get; } = colorBrush;

    public override string ToString()
    {
        return Name;
    }
}