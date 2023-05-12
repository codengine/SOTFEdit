using static SOTFEdit.Companion.Shared.PoiGroupType;

namespace SOTFEdit.Companion.Shared;

public static class PoiGroupTypeUtils
{
    public static PoiGroupType[] GetSupportedCompanionPoiGroupTypes()
    {
        return Enum.GetValues<PoiGroupType>()
            .Where(e => e is Custom or Items or WorldItems or Bunkers or Printers or Laptops or Caves or Villages
                or Helicopters or Doors or Lakes)
            .OrderBy(e => e.ToString())
            .ToArray();
    }
}