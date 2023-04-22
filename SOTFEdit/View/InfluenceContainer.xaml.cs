using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData.Actor;

namespace SOTFEdit.View;

[ObservableObject]
public partial class InfluenceContainer
{
    public static readonly DependencyProperty InfluencesTargetProperty = DependencyProperty.Register(
        nameof(InfluencesTarget), typeof(Collection<Influence>), typeof(InfluenceContainer),
        new PropertyMetadata(default(Collection<Influence>)));

    public static readonly DependencyProperty AllInfluencesProperty = DependencyProperty.Register(
        nameof(AllInfluences), typeof(IEnumerable<ComboBoxItemAndValue<string>>), typeof(InfluenceContainer),
        new PropertyMetadata(default(IEnumerable<ComboBoxItemAndValue<string>>)));

    public static readonly DependencyProperty ShowDisclaimerProperty = DependencyProperty.Register(
        nameof(ShowDisclaimer), typeof(bool), typeof(InfluenceContainer), new PropertyMetadata(true));

    public InfluenceContainer()
    {
        InitializeComponent();
    }

    public bool ShowDisclaimer
    {
        get => (bool)GetValue(ShowDisclaimerProperty);
        set => SetValue(ShowDisclaimerProperty, value);
    }

    public IEnumerable<ComboBoxItemAndValue<string>> AllInfluences
    {
        get => (IEnumerable<ComboBoxItemAndValue<string>>)GetValue(AllInfluencesProperty);
        set => SetValue(AllInfluencesProperty, value);
    }

    public Collection<Influence> InfluencesTarget
    {
        get => (Collection<Influence>)GetValue(InfluencesTargetProperty);
        set => SetValue(InfluencesTargetProperty, value);
    }

    [RelayCommand]
    private void AddInfluence(string influenceType)
    {
        if (string.IsNullOrEmpty(influenceType))
        {
            return;
        }

        var existingTypeId = InfluencesTarget.FirstOrDefault(influence => influence.TypeId == influenceType);
        if (existingTypeId != null)
        {
            return;
        }

        InfluencesTarget.Add(Influence.AsFillerWithDefaults(influenceType));
    }
}