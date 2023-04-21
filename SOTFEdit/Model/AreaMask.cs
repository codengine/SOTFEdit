using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model;

public class AreaMask
{
    public const int Surface = 1;

    public AreaMask(int mask)
    {
        Mask = mask;
    }

    public int Mask { get; }

    public string PrintableString => Translate(Mask);

    public static string Translate(int graphMask)
    {
        return graphMask switch
        {
            0 => "?",
            1 => TranslationManager.Get("actors.areaMask.surface"),
            < 0 or > 1 => TranslationManager.Get("actors.areaMask.caveOrBunker")
        };
    }
}