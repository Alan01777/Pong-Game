using Godot;

namespace JetBrainsTutorial.scripts;

public partial class Player : Area2D, IHasScore, ISmashable
{
    private CollisionShape2D _collisionShape;
    [Export] private Color _defaultColor = Colors.White; // Cor padrão da raquete
    private float _halfPlayerHeight;

    [Export] private int _moveSpeed = 200;
    private Vector2 _originalPositionX; // Para guardar a posição X original da raquete
    private Sprite2D _playerSprite; // Para mudar a cor
    [Export] private Color _smashColor = Colors.Red; // Cor para indicar o smash ativo

    // --- propriedades para a mecânica de jump/smash ---
    [Export] private float _smashDashDistance = 50f; // Distância que a raquete avança para o smash
    [Export] private float _smashDuration = 0.15f; // Duração do dash (curta para ser um "tap")
    private AudioStreamPlayer SmashSound { get; set; }
    public AudioStreamPlayer ScoreSound { get; private set; }

    [Export] public Label ScoreDisplay { get; set; }
    public int Score { get; set; }

    public bool IsSmashing { get; private set; }

    [Export]
    public float SmashBallSpeedMultiplier { get; private set; } = 1.2f; // Multiplicador de velocidade para a bola

    // Crie este novo método para encapsular a lógica de ativação do smash
    public void ActivateSmash() // Implementação da interface ISmashable
    {
        IsSmashing = true;
        SmashSound.Play();
        var smashTween = CreateTween();
        smashTween.TweenProperty(this, "position:x", _originalPositionX.X + _smashDashDistance, _smashDuration);
        smashTween.TweenProperty(this, "position:x", _originalPositionX.X, _smashDuration);
        smashTween.Connect("finished", new Callable(this, nameof(OnSmashFinished)));

        if (_playerSprite != null) _playerSprite.Modulate = _smashColor;
    }

    public override void _Ready()
    {
        ScoreSound = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        SmashSound = GetNode<AudioStreamPlayer>("Smash");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _halfPlayerHeight = _collisionShape.Shape.GetRect().Size.Y / 2;

        _playerSprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        _originalPositionX = Position; // Guarda a posição X inicial
    }

    public override void _PhysicsProcess(double delta)
    {
        // Lógica de movimento vertical (ui_up/ui_down)
        var inputY = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
        var currentPosition = Position;
        currentPosition.Y += (float)(inputY * _moveSpeed * delta);

        // Limita a posição Y
        var minLimit = _halfPlayerHeight;
        var maxLimit = GetViewportRect().Size.Y - _halfPlayerHeight;
        currentPosition.Y = Mathf.Clamp(currentPosition.Y, minLimit, maxLimit);

        // --- Lógica de ativação do Smash ---
        if (Input.IsActionJustPressed("player_smash") && !IsSmashing) // Use IsSmashing
            ActivateSmash();

        // A posição Y é sempre atualizada, enquanto a posição X depende do estado de "smash".
        Position = new Vector2(!IsSmashing ? _originalPositionX.X : Position.X, currentPosition.Y);
    }

    private void OnSmashFinished()
    {
        IsSmashing = false; // Use IsSmashing
        if (_playerSprite != null) _playerSprite.Modulate = _defaultColor;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not Ball ball) return;

        // --- Lógica de rebatida com smash ---
        Vector2 direction;
        if (IsSmashing)
        {
            // O sinal do X depende se é Player (Right.X = 1) ou Enemy (Left.X = -1)
            var xDir = Name == "Player" ? Vector2.Right.X : Vector2.Left.X;
            var yDir = (ball.Position.Y - Position.Y) /
                       (_halfPlayerHeight * 2); // Mais longe do centro = mais inclinado
            direction = new Vector2(xDir, yDir).Normalized();

            // Aplica o multiplicador de velocidade
            ball.MoveSpeed *= SmashBallSpeedMultiplier;
        }
        else
        {
            // Lógica de rebatida normal
            var xDir = Name == "Player" ? Vector2.Right.X : Vector2.Left.X;
            var yDir = (ball.Position.Y - Position.Y) / (_halfPlayerHeight * 2);
            direction = new Vector2(xDir, yDir).Normalized();
        }

        ball.Bounce(direction);
    }
}