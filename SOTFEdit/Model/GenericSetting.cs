using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model;

public partial class GenericSetting : ObservableObject
{
    public enum DataType
    {
        ReadOnly,
        String,
        Boolean,
        Integer,
        Enum
    }

    [ObservableProperty]
    private bool? _boolValue;

    [ObservableProperty]
    private int? _intValue = 0;

    [ObservableProperty]
    private object? _selectedItem;

    [ObservableProperty]
    private string? _stringValue;

    public GenericSetting(string name, DataType type, string? dataPath = null)
    {
        Name = name;
        DataPath = dataPath;
        Type = type;
    }

    public string Name { get; }
    public string? DataPath { get; }
    public DataType Type { get; }

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