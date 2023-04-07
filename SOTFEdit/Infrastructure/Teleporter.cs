using SOTFEdit.Model;

// ReSharper disable RedundantAssignment

namespace SOTFEdit.Infrastructure;

public static class Teleporter
{
    private const int TeleportYoffset = 3;

    public static void MovePlayerToPos(ref Position player, ref Position target)
    {
        player = target.WithoutOffset();
        target = target.WithYOffset(TeleportYoffset);
    }

    public static Position MoveToPos(Position targetPos)
    {
        return targetPos.WithYOffset(TeleportYoffset);
    }
}