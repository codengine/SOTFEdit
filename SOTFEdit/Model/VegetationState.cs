using System;

namespace SOTFEdit.Model;

[Flags]
public enum VegetationState : short
{
    None = 0,
    Gone = 1,
    HalfChopped = 2,
    Stumps = 4
}