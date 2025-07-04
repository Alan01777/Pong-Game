using Godot;

namespace JetBrainsTutorial.scripts;

public partial class Wall : Area2D
{
    private bool _canScore = true;

    [Export] private float _resetDelay = 1.5f;
    [Export] public Vector2 BallResetDirection = Vector2.Left;
    [Export] public Node2D Scorer { get; set; }
    [Export] public GoalMessage GoalMessageDisplay { get; set; }

    public override void _Ready()
    {
        if (Scorer == null) GD.PrintErr("Wall: Scorer not assigned!");
        if (GoalMessageDisplay == null) GD.PrintErr("Wall: GoalMessageDisplay not assigned!");
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not Ball ball) return;
        if (Scorer is not IHasScore scoring) return;
        if (!_canScore) return;

        _canScore = false;

        scoring.IncrementScore();

        if (GoalMessageDisplay != null) GoalMessageDisplay.ShowGoalMessage();

        ball.Visible = false;
        ball.MoveSpeed = 0;

        var timer = GetTree().CreateTimer(_resetDelay);
        timer.Connect("timeout", Callable.From(() => OnResetTimerTimeout(ball)));
    }

    private void OnResetTimerTimeout(Ball ball)
    {
        ball.Visible = true;
        ball.Reset(BallResetDirection);
        ball.MoveSpeed = 1000;

        _canScore = true;

        if (GoalMessageDisplay is { Visible: true }) GoalMessageDisplay.HideGoalMessage();
    }
}