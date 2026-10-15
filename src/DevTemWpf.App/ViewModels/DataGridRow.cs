using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Resources;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// One editable grid row. Validation flows through
/// <see cref="ObservableValidator"/> so WPF picks it up as
/// <see cref="System.ComponentModel.IDataErrorInfo"/> (cell + row errors).
/// Messages resolve through <see cref="Strings"/>.
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
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.GridRow_NameRequired))]
    [MinLength(2, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.GridRow_NameMin))]
    private string name = Strings.Data_NewItem;

    [ObservableProperty]
    private string status = "Open";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, 10000, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.GridRow_ValueRange))]
    private int value;
}
