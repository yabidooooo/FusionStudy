using Fusion;

public static class AnimationHelper
{
    public static double GetAnimationTimeProxy( NetworkRunner runner, int startTick )
    {
        return GetAnimationTimeProxy( runner ) - startTick * runner.DeltaTime;
    }
    public static double GetAnimationTimeProxy( NetworkRunner runner )
    {
        return runner.Simulation.InterpFrom.Time + runner.Simulation.InterpAlpha * runner.DeltaTime;
    }
    public static double GetAnimationTime( NetworkRunner runner )
    {
        return runner.Simulation.State.Time + runner.Simulation.StateAlpha * runner.DeltaTime;
    }
    public static double GetAnimationTime( NetworkRunner runner, int startTick )
    {
        return GetAnimationTime( runner ) - startTick * runner.DeltaTime;
    }
}
