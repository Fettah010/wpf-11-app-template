using DevTemWpf.App.Services;
using Xunit;

namespace DevTemWpf.Unit;

/// <summary>
/// Behavioral unit tests for ISampleService implementations:
/// exercises service methods, not merely file-existence assertions.
/// </summary>
public sealed class SampleServiceTests
{
    [Fact]
    public void Describe_ReturnsNonEmptyDescription()
    {
        ISampleService service = new SampleService();

        var result = service.Describe();

        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("service", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Describe_IsDeterministic()
    {
        ISampleService service = new SampleService();

        var first = service.Describe();
        var second = service.Describe();

        Assert.Equal(first, second);
    }
}
