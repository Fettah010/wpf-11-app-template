using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Resources;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Sample validated form (DataAnnotations + ObservableValidator, no new packages).
/// Messages resolve through <see cref="Strings"/> so they follow the UI culture.
/// </summary>
public sealed partial class FormViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Form_NameRequired))]
    [MinLength(2, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Form_NameMin))]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Form_EmailRequired))]
    [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Form_EmailInvalid))]
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

        SavedMessage = string.Format(Strings.Form_SavedFormat, Name.Trim(), Email.Trim());
    }
}
