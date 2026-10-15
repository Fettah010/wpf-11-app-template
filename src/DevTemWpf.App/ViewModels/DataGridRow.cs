using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// One editable grid row. Validation flows through
/// <see cref="ObservableValidator"/> so WPF picks it up as
/// <see cref="System.ComponentModel.IDataErrorInfo"/> (cell + row errors).
/// </summary>
public sealed partial class DataGridRow : ObservableValidator
{
    public DataGridRow()
    {
    }

    public DataGridRow(string name, string status, int value)
    {
        Name = name;
        Status = status;
        Value = value;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(2, ErrorMessage = "Name needs at least 2 characters.")]
    private string name = "New item";

    [ObservableProperty]
    private string status = "Open";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, 10000, ErrorMessage = "Value must be between 0 and 10000.")]
    private int value;
}
