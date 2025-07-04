using Godot;

namespace JetBrainsTutorial.scripts;

public partial class Pause : RichTextLabel
{
    public override void _Process(double delta)
    {
        if (!Input.IsActionJustPressed("ui_cancel")) return;

        GD.Print("Pausing game");

        if (Visible)
        {
            Hide();
            GetTree().Paused = false;
        }
        else
        {
            Show();
            GetTree().Paused = true;
        }
    }
}