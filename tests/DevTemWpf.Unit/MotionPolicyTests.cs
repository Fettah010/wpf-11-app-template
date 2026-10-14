using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class MotionPolicyTests
{
    [Theory]
    [InlineData(MotionMode.Auto, true, 2, EffectiveMotion.Full)]
    [InlineData(MotionMode.Auto, false, 2, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Auto, true, 0, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Auto, false, 0, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Full, false, 0, EffectiveMotion.Full)]
    [InlineData(MotionMode.Reduced, true, 2, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Off, true, 2, EffectiveMotion.Off)]
    public void Resolve_MapsPreferenceAndEnvironment(
        MotionMode mode, bool osAnimations, int tier, EffectiveMotion expected)
    {
        Assert.Equal(expected, MotionPolicy.Resolve(mode, osAnimations, tier));
    }
}
