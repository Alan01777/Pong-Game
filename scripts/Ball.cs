using Godot;

namespace JetBrainsTutorial.scripts;

public partial class Ball : Area2D
{
    private static readonly Vector2 StartingPoint = new() { X = 640, Y = 360 };
    private AudioStreamPlayer _bounceSound;
    [Export] public Vector2 Direction = Vector2.Right;
    [Export] public double MoveSpeed = 400;

    public override void _Ready()
    {
        _bounceSound = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        var startAngle = (float)GD.RandRange(-45, 45); // Ângulo entre -45 e 45 graus

        // Escolhe aleatoriamente se vai para a esquerda ou direita no início
        Direction = GD.RandRange(0, 1) == 0
            ? new Vector2(-1, 0).Rotated(Mathf.DegToRad(startAngle)).Normalized()
            : new Vector2(1, 0).Rotated(Mathf.DegToRad(startAngle)).Normalized();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (MoveSpeed > 0)
            Position = Position with
            {
                X = (float)(Position.X + MoveSpeed * delta * Direction.X),
                Y = (float)(Position.Y + MoveSpeed * delta * Direction.Y)
            };
    }

    public void Reset(Vector2 direction)
    {
        Direction = direction;
        Position = StartingPoint;
        MoveSpeed = 1000;

        var resetAngleOffset = (float)GD.RandRange(-15, 15);
        Direction = Direction.Rotated(Mathf.DegToRad(resetAngleOffset)).Normalized();
    }

    public void Bounce(Vector2 direction)
    {
        Direction = direction;
        _bounceSound.Play();
    }
}