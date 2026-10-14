using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class SampleDataTests
{
    [Fact]
    public void Activities_DefaultCount_Is1500()
    {
        Assert.Equal(1500, new SampleDataService().GetActivities().Count);
    }

    [Fact]
    public void Activities_AreDeterministic()
    {
        var service = new SampleDataService();

        Assert.Equal(service.GetActivities(), service.GetActivities());
    }

    [Fact]
    public void WeeklySeries_HasSevenPointsAndLabels()
    {
        var service = new SampleDataService();

        Assert.Equal(7, service.GetWeeklyValues().Count);
        Assert.Equal(7, service.GetWeekdayLabels().Count);
        Assert.Equal(4, service.GetMetrics().Count);
    }
}
