namespace JetBrainsTutorial.scripts;

public interface ISmashable
{
    bool IsSmashing { get; }
    float SmashBallSpeedMultiplier { get; }
    void ActivateSmash();
}