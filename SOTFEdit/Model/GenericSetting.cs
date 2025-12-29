using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model;

public partial class GenericSetting(string name, GenericSetting.DataType type, string? dataPath = null)
    : ObservableObject
{
    public enum DataType
    {
        ReadOnly,
        String,
        Boolean,
        Integer,
        Enum
    }

    [ObservableProperty] private bool? _boolValue;

    [ObservableProperty] private int? _intValue = 0;

    [ObservableProperty] private object? _selectedItem;

    [ObservableProperty] private string? _stringValue;

    public string Name { get; } = name;
    public string? DataPath { get; } = dataPath;
    public DataType Type { get; } = type;

    public Dictionary<object, string> PossibleValues { get; } = new();
    public int MinInt { get; init; }
    public int MaxInt { get; init; } = 1;

    public object? GetValue()
    {
        return Type switch
        {
            DataType.ReadOnly => StringValue,
            DataType.String => StringValue,
            DataType.Boolean => BoolValue,
            DataType.Integer => IntValue,
            DataType.Enum => SelectedItem,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}