using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class MotionPolicyTests
{
    [Theory]
    [InlineData(MotionMode.Auto, true, 2, EffectiveMotion.Balanced)]
    [InlineData(MotionMode.Auto, false, 2, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Auto, true, 0, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Auto, false, 0, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Full, false, 0, EffectiveMotion.Full)]
    [InlineData(MotionMode.Balanced, false, 0, EffectiveMotion.Balanced)]
    [InlineData(MotionMode.Reduced, true, 2, EffectiveMotion.Reduced)]
    [InlineData(MotionMode.Off, true, 2, EffectiveMotion.Off)]
    public void Resolve_MapsPreferenceAndEnvironment(
        MotionMode mode, bool osAnimations, int tier, EffectiveMotion expected)
    {
        Assert.Equal(expected, MotionPolicy.Resolve(mode, osAnimations, tier));
    }

    [Theory]
    [InlineData(EffectiveMotion.Off, false)]
    [InlineData(EffectiveMotion.Reduced, false)]
    [InlineData(EffectiveMotion.Balanced, true)]
    [InlineData(EffectiveMotion.Full, true)]
    public void AllowsMovement_TrueFromBalancedUp(EffectiveMotion policy, bool expected)
    {
        Assert.Equal(expected, MotionPolicy.AllowsMovement(policy));
    }

    [Theory]
    [InlineData(EffectiveMotion.Off, false)]
    [InlineData(EffectiveMotion.Reduced, false)]
    [InlineData(EffectiveMotion.Balanced, false)]
    [InlineData(EffectiveMotion.Full, true)]
    public void AllowsDecorative_OnlyAtRich(EffectiveMotion policy, bool expected)
    {
        Assert.Equal(expected, MotionPolicy.AllowsDecorative(policy));
    }
}
