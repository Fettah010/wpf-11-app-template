using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Sample validated form (DataAnnotations + ObservableValidator, no new packages).
/// </summary>
public sealed partial class FormViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(2, ErrorMessage = "Name needs at least 2 characters.")]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email does not look valid.")]
    private string email = string.Empty;

    [ObservableProperty]
    private string? savedMessage;

    [ObservableProperty]
    private string? nameError;

    [ObservableProperty]
    private string? emailError;

    partial void OnNameChanged(string value)
    {
        ValidateProperty(value, nameof(Name));
        NameError = GetErrors(nameof(Name)).OfType<ValidationResult>().FirstOrDefault()?.ErrorMessage;
    }

    partial void OnEmailChanged(string value)
    {
        ValidateProperty(value, nameof(Email));
        EmailError = GetErrors(nameof(Email)).OfType<ValidationResult>().FirstOrDefault()?.ErrorMessage;
    }

    public bool CanSave() => !HasErrors;

    [RelayCommand]
    private void Save()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            SavedMessage = null;
            return;
        }

        SavedMessage = $"Saved {Name.Trim()} <{Email.Trim()}> (sample — nothing leaves this machine).";
    }
}
