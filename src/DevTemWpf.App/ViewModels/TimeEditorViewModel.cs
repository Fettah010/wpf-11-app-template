using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Resources;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Gallery-scoped time editor (1.3.0). The WPF-UI TimePicker exists in 4.3.0
/// but neither surfaces its automation name nor opens its popup reliably
/// (spike evidence in the 1.3.0 report), so this recipe composes two validated
/// inputs instead of adopting a broken control or a new theme suite.
/// </summary>
public sealed partial class TimeEditorViewModel : ObservableValidator
{
    public TimeEditorViewModel()
    {
        Hour = 12;
        Minute = 0;
        UpdateSelected();
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, 23, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Time_HourRange))]
    private int hour;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, 59, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Time_MinuteRange))]
    private int minute;

    [ObservableProperty]
    private TimeSpan selectedTime = new(12, 0, 0);

    [ObservableProperty]
    private string? timeError;

    public string Summary => string.Format(Strings.Time_SummaryFormat, SelectedTime.ToString(@"hh\:mm"));

    partial void OnHourChanged(int value) => ValidateAndUpdate(nameof(Hour), value);

    partial void OnMinuteChanged(int value) => ValidateAndUpdate(nameof(Minute), value);

    [RelayCommand]
    private void SetNow()
    {
        var now = DateTime.Now;
        Hour = now.Hour;
        Minute = now.Minute;
    }

    private void ValidateAndUpdate(string property, int value)
    {
        ValidateProperty(value, property);
        TimeError = GetErrors(nameof(Hour)).OfType<ValidationResult>().FirstOrDefault()?.ErrorMessage
            ?? GetErrors(nameof(Minute)).OfType<ValidationResult>().FirstOrDefault()?.ErrorMessage;
        UpdateSelected();
    }

    private void UpdateSelected()
    {
        if (!HasErrors)
        {
            SelectedTime = new TimeSpan(Hour, Minute, 0);
        }

        OnPropertyChanged(nameof(Summary));
    }
}
