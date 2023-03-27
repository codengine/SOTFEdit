using System;
using System.Diagnostics.CodeAnalysis;

namespace SOTFEdit.Model.SaveData.Settings;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public record GameSettingFullModel
{
    public string Name { get; set; }
    public int SettingsType { get; set; } = Constants.Settings.SettingTypeString;
    public int Version { get; set; } = 0;
    public bool BoolValue { get; set; } = false;
    public int IntValue { get; set; } = 0;
    public decimal FloatValue { get; set; } = new(0.0);
    public string StringValue { get; set; }
    public bool Protected { get; set; } = false;
    public decimal[] FloatArrayValue { get; set; } = Array.Empty<decimal>();
    public bool IsSet { get; set; } = false;
}