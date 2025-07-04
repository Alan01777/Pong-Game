using Godot;

// Necessário para Mathf.Abs e GD.Randf()

namespace JetBrainsTutorial.scripts;

public partial class Enemy : Area2D, IHasScore, ISmashable // Implementa ISmashable
{
    private CollisionShape2D _collisionShape;
    [Export] private Color _defaultColor = Colors.White;
    private Sprite2D _enemySprite;
    [Export] private Area2D _follow; // A bola que o inimigo segue
    private float _halfEnemyHeight;

    [Export] private float _moveSpeed = 400.0f; // Velocidade de movimento vertical da IA
    private Vector2 _originalPositionX; // Usado para a posição X original, renomeado para ser mais claro

    // Propriedades para a IA do smash
    [Export] private float _smashChance = 0.3f; // Chance de smash quando as condições são atendidas
    [Export] private Color _smashColor = Colors.Blue;
    [Export] private float _smashCooldown = 2.0f; // Tempo de recarga do smash
    private Timer _smashCooldownTimer;

    // Propriedades da animação do smash
    [Export] private float _smashDashDistance = 50f;
    [Export] private float _smashDuration = 0.15f;
    private AudioStreamPlayer SmashSound { get; set; } // Propriedade de áudio, obtida em _Ready

    // Propriedades de Score (já existentes)
    public int Score { get; set; }
    [Export] public Label ScoreDisplay { get; set; }
    public AudioStreamPlayer ScoreSound { get; private set; }

    // Implementação da interface ISmashable (agora é pública para a interface)
    public bool IsSmashing { get; private set; }

    public float SmashBallSpeedMultiplier { get; private set; } = 1.2f; // Multiplicador de velocidade para a bola

    // --- Implementação do método da interface ISmashable ---
    public void ActivateSmash()
    {
        if (IsSmashing) return; // Evita ativar múltiplas vezes se já estiver smashando

        IsSmashing = true; // Define o estado de smash
        _smashCooldownTimer.Start(); // Inicia o cooldown
        SmashSound?.Play(); // Toca o som

        if (_enemySprite != null) _enemySprite.Modulate = _smashColor; // Altera a cor

        // Cria e inicia o Tween para o movimento X da raquete
        var smashTween = CreateTween();
        // Move para frente (esquerda para o inimigo)
        smashTween.TweenProperty(this, "position:x", _originalPositionX.X - _smashDashDistance, _smashDuration / 2.0f)
            .SetEase(Tween.EaseType.OutIn);
        // Move de volta para a posição original
        smashTween.TweenProperty(this, "position:x", _originalPositionX.X, _smashDuration / 2.0f)
            .SetEase(Tween.EaseType.OutIn);

        // Conecta o sinal 'finished' do Tween para resetar o estado de smash
        smashTween.Connect("finished", new Callable(this, nameof(OnSmashFinished)));
    }

    public override void _Ready()
    {
        ScoreSound = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        SmashSound = GetNode<AudioStreamPlayer>("Smash"); // Certifique-se de que o nó de som se chame "Smash"
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _halfEnemyHeight = _collisionShape.Shape.GetRect().Size.Y / 2;

        _enemySprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        _originalPositionX = Position; // Guarda a posição X inicial da raquete

        _smashCooldownTimer = new Timer
        {
            WaitTime = _smashCooldown,
            OneShot = true // Roda uma vez e para
        };
        AddChild(_smashCooldownTimer);
        // Conecte o sinal aqui, se ainda não estiver conectado no editor
        _smashCooldownTimer.Connect("timeout", new Callable(this, nameof(OnSmashCooldownTimeout)));

        // Certifica que o cooldown começa parado
        _smashCooldownTimer.Stop();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_follow == null) return; // Garante que há uma bola para seguir

        // --- Lógica de Movimento Vertical (mantida) ---
        // A raquete sempre segue a bola no eixo Y
        var targetY = _follow.Position.Y;
        var newY = Mathf.MoveToward(Position.Y, targetY, _moveSpeed * (float)delta);
        var clampedY = Mathf.Clamp(newY, _halfEnemyHeight, GetViewportRect().Size.Y - _halfEnemyHeight);

        // --- Lógica de Decisão da IA para Smash ---
        // A IA tenta ativar o smash ANTES da colisão
        if (!IsSmashing && _smashCooldownTimer.IsStopped() && _follow is Ball ball)
        {
            var ballIsComingTowardsEnemy = ball.Direction.X < 0; // Bola indo para a esquerda (inimigo)

            // Define o X da raquete do inimigo no lado direito
            var enemyPaddleX = _originalPositionX.X; // Usar _originalPositionX.X para o cálculo de range
            // porque a raquete pode estar em dash.

            // A bola deve estar à esquerda da raquete do inimigo (mas não na parede de gol do jogador)
            // e numa faixa de "previsão" para o smash
            var ballIsInSmashRangeX =
                ball.Position.X < enemyPaddleX + _smashDashDistance && // Bola à frente ou na distância de dash
                ball.Position.X > GetViewportRect().Size.X / 2; // E no lado do inimigo da tela (metade direita)

            // A bola deve estar verticalmente alinhada com a raquete (para evitar smashes "vazios")
            var ballIsInSmashRangeY = Mathf.Abs(ball.Position.Y - Position.Y) < _halfEnemyHeight * 1.5f;

            // Se todas as condições são verdadeiras e a chance aleatória permite
            if (ballIsComingTowardsEnemy && ballIsInSmashRangeX && ballIsInSmashRangeY && GD.Randf() < _smashChance)
                ActivateSmash(); // Ativa o smash!
        }

        // --- Atualiza a Posição X e Y da Raquete ---
        // A posição Y é sempre atualizada, enquanto a posição X depende do estado de "smash".
        Position = new Vector2(!IsSmashing ? _originalPositionX.X : Position.X, clampedY);
    }

    private void OnSmashFinished()
    {
        IsSmashing = false; // Sai do estado de smash
        if (_enemySprite != null) _enemySprite.Modulate = _defaultColor; // Retorna a cor normal
        // Garante que a posição X volte exatamente para a original (importante após o tween)
        Position = new Vector2(_originalPositionX.X, Position.Y);
    }

    private void OnSmashCooldownTimeout()
    {
        //
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not Ball ball) return;
        // Direção X é sempre para a esquerda para o inimigo
        var xDir = Vector2.Left.X;
        // Direção Y é baseada na posição da bola em relação ao centro da raquete
        var yDir = (ball.Position.Y - Position.Y) /
                   (_halfEnemyHeight * 2);
        var direction = new Vector2(xDir, yDir).Normalized();

        // Se a raquete está atualmente no estado de "smash", aplique o multiplicador de velocidade
        if (IsSmashing)
            ball.MoveSpeed *= SmashBallSpeedMultiplier;

        ball.Bounce(direction);
    }
}