namespace DevTemWpf.App.Models;

/// <summary>Sample row model for DataGrid views.</summary>
public sealed record SampleDataGridRow(int Id, string Code, string Description, string Status, int Quantity, DateTime UpdatedAt);
