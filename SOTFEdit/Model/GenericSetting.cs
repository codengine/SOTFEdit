using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model;

[ObservableObject]
public partial class GenericSetting
{
    public enum DataType
    {
        String,
        Boolean,
        Integer,
        Enum
    }

    public string Name { get; }
    public string DataPath { get; }
    public DataType Type { get; }

    [ObservableProperty] private string? _stringValue;
    [ObservableProperty] private int? _intValue;
    [ObservableProperty] private bool? _boolValue;

    public Dictionary<object, string> PossibleValues { get; init; } = new();
    public int MinInt { get; init; } = 0;
    public int MaxInt { get; init; } = 0;

    [ObservableProperty] private object? _selectedItem;

    public GenericSetting(string name, string dataPath, DataType type)
    {
        Name = name;
        DataPath = dataPath;
        Type = type;
    }

    public object? GetValue()
    {
        return Type switch
        {
            DataType.String => StringValue,
            DataType.Boolean => BoolValue,
            DataType.Integer => IntValue,
            DataType.Enum => SelectedItem,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}