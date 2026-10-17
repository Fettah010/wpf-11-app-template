using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

/// <summary>
/// Repository-only scaffold-default assertions. The <c>DevTemWpfShellDefault</c>
/// token is replaced at scaffold time, so the default resolves to the
/// <c>--Shell</c> choice in generated apps and only falls back to Sidebar in
/// this repository (where the token stays literal). This file is excluded from
/// the template (see <c>.template.config/template.json</c>) so a generated app
/// with a non-Sidebar shell does not inherit a false assertion.
/// </summary>
public sealed class ScaffoldDefaultTests
{
    [Fact]
    public void ShellDefault_FallsBackToSidebar_InRepo()
        => Assert.Equal(ShellMode.Sidebar, new ShellSettings().Shell);
}
