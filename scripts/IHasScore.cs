using Godot;

namespace JetBrainsTutorial.scripts;

public interface IHasScore
{
    StringName Name { get; }
    int Score { get; set; }
    Label ScoreDisplay { get; set; }
    AudioStreamPlayer ScoreSound { get; }

    public void IncrementScore()
    {
        Score++;
        if (ScoreDisplay is null) return;
        ScoreDisplay.Text = $"{Score}";
        GD.Print($"{Name} scored. Currently at {Score}");
        ScoreSound?.Play();
    }
}