namespace DevTemWpf.App.Services;

/// <summary>
/// SampleService implementation. After scaffolding, register in AppComposition:
/// <code>builder.Services.AddSingleton&lt;ISampleService, SampleService&gt;();</code>
/// </summary>
public sealed class SampleService : ISampleService
{
    public string Describe() => "Sample service is alive.";
}
