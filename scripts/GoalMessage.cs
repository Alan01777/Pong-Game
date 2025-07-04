using Godot;

namespace JetBrainsTutorial.scripts;

public partial class GoalMessage : RichTextLabel
{
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        Text = "[center][b][color=white]GOAL![/color][/b][/center]";
        Visible = false;
        _animationPlayer = GetNode<AnimationPlayer>("GoalAnimation");
    }

    public void ShowGoalMessage()
    {
        Visible = true;
        if (_animationPlayer != null && _animationPlayer.HasAnimation("GoalMessageAnimation"))
            _animationPlayer.Play("GoalMessageAnimation");
        else
            GD.PrintErr("Animation 'GoalMessageAnimation' not found on AnimationPlayer or AnimationPlayer is null.");


        var timer = GetTree().CreateTimer(2.0f);
        timer.Connect("timeout", new Callable(this, nameof(HideGoalMessage)));
    }

    public void HideGoalMessage()
    {
        Visible = false;
    }
}