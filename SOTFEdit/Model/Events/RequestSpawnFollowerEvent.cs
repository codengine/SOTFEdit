﻿using System.Collections.Generic;

namespace SOTFEdit.Model.Events;

public record RequestSpawnFollowerEvent(Savegame Savegame, int TypeId, HashSet<int> ItemIds, Outfit? Outfit, Position Pos, bool CreateBackup);