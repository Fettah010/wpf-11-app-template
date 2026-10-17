using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class MotionScopeTests
{
    [Fact]
    public void StopAll_RunsRegistrationsInReverseOrder()
    {
        var scope = new MotionScope();
        var order = new List<int>();
        scope.Register(() => order.Add(1));
        scope.Register(() => order.Add(2));
        scope.Register(() => order.Add(3));

        scope.StopAll();

        Assert.Equal(new[] { 3, 2, 1 }, order);
    }

    [Fact]
    public void StopAll_CancelsCurrentToken_ThenReArms()
    {
        var scope = new MotionScope();
        var token = scope.Token;

        scope.StopAll();

        Assert.True(token.IsCancellationRequested);
        Assert.False(scope.IsStopped);
        Assert.False(scope.Token.IsCancellationRequested);
    }

    [Fact]
    public void StopAll_SecondCall_DoesNotRerunRegistrations()
    {
        var scope = new MotionScope();
        var count = 0;
        scope.Register(() => count++);

        scope.StopAll();
        scope.StopAll();

        Assert.Equal(1, count);
    }

    [Fact]
    public void DisposedRegistration_IsNotInvoked()
    {
        var scope = new MotionScope();
        var invoked = false;
        var registration = scope.Register(() => invoked = true);

        registration.Dispose();
        scope.StopAll();

        Assert.False(invoked);
    }

    [Fact]
    public void Dispose_StopsScope_AndRunsLateRegistrationImmediately()
    {
        var scope = new MotionScope();

        scope.Dispose();
        Assert.True(scope.IsStopped);
        Assert.True(scope.Token.IsCancellationRequested);

        var invoked = false;
        var registration = scope.Register(() => invoked = true);
        Assert.True(invoked);
        registration.Dispose();

        scope.StopAll();
    }

    [Fact]
    public void BrokenRegistration_DoesNotBlockOthers()
    {
        var scope = new MotionScope();
        var reached = false;
        scope.Register(() => reached = true);
        scope.Register(() => throw new InvalidOperationException("boom"));

        scope.StopAll();

        Assert.True(reached);
    }

    [Fact]
    public void Register_Null_Throws()
    {
        var scope = new MotionScope();
        Assert.Throws<ArgumentNullException>(() => scope.Register(null!));
    }
}
