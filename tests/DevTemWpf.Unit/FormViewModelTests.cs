using DevTemWpf.App.ViewModels;

namespace DevTemWpf.Unit;

public sealed class FormViewModelTests
{
    [Fact]
    public void ShortName_FailsValidation_WithMessage()
    {
        var form = new FormViewModel { Name = "A", Email = "ada@example.com" };

        Assert.True(form.HasErrors);
        Assert.Contains("at least 2", form.NameError);
        Assert.Null(form.EmailError);
    }

    [Fact]
    public void BadEmail_FailsValidation_WithMessage()
    {
        var form = new FormViewModel { Name = "Ada", Email = "not-an-email" };

        Assert.True(form.HasErrors);
        Assert.Contains("valid", form.EmailError);
    }

    [Fact]
    public void ValidForm_Saves_WithSummary()
    {
        var form = new FormViewModel { Name = "Ada", Email = "ada@example.com" };

        Assert.False(form.HasErrors);
        Assert.True(form.CanSave());

        form.SaveCommand.Execute(null);

        Assert.Contains("Ada", form.SavedMessage);
        Assert.Contains("ada@example.com", form.SavedMessage);
    }

    [Fact]
    public void FixingInput_ClearsError()
    {
        var form = new FormViewModel { Name = "A", Email = "bad" };
        Assert.True(form.HasErrors);

        form.Name = "Ada";
        form.Email = "ada@example.com";

        Assert.False(form.HasErrors);
        Assert.Null(form.NameError);
        Assert.Null(form.EmailError);
    }

    [Fact]
    public void InvalidForm_Save_ClearsResult()
    {
        var form = new FormViewModel { Name = "", Email = "" };

        form.SaveCommand.Execute(null);

        Assert.True(form.HasErrors);
        Assert.Null(form.SavedMessage);
    }
}
