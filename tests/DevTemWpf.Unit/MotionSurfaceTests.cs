namespace DevTemWpf.Unit;

/// <summary>
/// 1.5.0 repo-only guard: motion must stay a set of policy-gated,
/// interruptible storyboards owned by <c>MotionScope</c>. Nothing may subscribe
/// to <c>CompositionTarget.Rendering</c> (a per-frame hook that would survive
/// navigation), and the lifecycle scope must stay registered and used.
/// Repository-only (excluded from the template).
/// </summary>
public sealed class MotionSurfaceTests
{
    private static string AppSource()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        var path = Path.Combine(dir.FullName, "src", "DevTemWpf.App");
        Assert.True(Directory.Exists(path), "src/DevTemWpf.App not found — run from a repo checkout.");
        return path;
    }

    private static IEnumerable<string> SourceFiles()
        => Directory
            .EnumerateFiles(AppSource(), "*.cs", SearchOption.AllDirectories)
            .Where(p =>
                !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

    [Fact]
    public void AppSource_DoesNotSubscribeToCompositionTargetRendering()
    {
        var offenders = SourceFiles()
            .Where(p => File.ReadAllText(p).Contains("CompositionTarget.Rendering", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void AppComposition_RegistersTheMotionScope()
    {
        var composition = File.ReadAllText(Path.Combine(AppSource(), "AppComposition.cs"));
        Assert.Contains("AddSingleton<MotionScope>()", composition, StringComparison.Ordinal);
    }

    [Fact]
    public void EffectsPage_RegistersItsLoopWithTheMotionScope()
    {
        var page = File.ReadAllText(Path.Combine(AppSource(), "Views", "EffectsPage.xaml.cs"));
        Assert.Contains("_scope.Register", page, StringComparison.Ordinal);
    }

    [Fact]
    public void AppSource_UsesNoRenderTargetEffects()
    {
        // Render-target Effects (Blur/DropShadow/Shader) force an intermediate
        // surface that disables ClearType for their subtree (ADR-0016). Motion
        // must stay storyboard/brush-only so text clarity is unaffected.
        var forbidden = new[]
        {
            "ShaderEffect", "PixelShader", "DropShadowEffect", "BlurEffect", "Effect=",
        };

        var offenders = SourceFiles()
            .Concat(Directory.EnumerateFiles(
                Path.Combine(AppSource(), "Views"), "*.xaml", SearchOption.AllDirectories))
            .Where(p => forbidden.Any(f => File.ReadAllText(p).Contains(f, StringComparison.Ordinal)))
            .ToArray();

        Assert.Empty(offenders);
    }
}
