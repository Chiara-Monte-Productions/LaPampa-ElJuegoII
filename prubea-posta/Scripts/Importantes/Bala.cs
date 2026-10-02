using Godot;

public partial class Bala : Area2D
{
    [Export] public float Gravedad = 980.0f;
    [Export] public float TiempoDeVida = 5.0f;

    private Vector2 _velocidadVector;
    private float _tiempoEnAire = 0.0f;

    public void Inicializar(float fuerza)
    {
        _velocidadVector = Transform.X * fuerza;
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        GetTree().CreateTimer(TiempoDeVida).Timeout += QueueFree;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _tiempoEnAire += dt;

        _velocidadVector.Y += Gravedad * dt;
        Position += _velocidadVector * dt;
        Rotation = _velocidadVector.Angle();
    }

    private void OnBodyEntered(Node2D body)
    {
        // Evita que explote con el propio jugador durante los primeros 0.1s de vuelo
        if (_tiempoEnAire < 0.1f && body is Player) return;

        // Si impacta contra un jugador, lo destruye
        if (body is Player jugador)
        {
            GD.Print("¡Jugador destruido!");
            jugador.QueueFree();
        }

        // Se destruye al tocar cualquier superficie sólida o jugador
        QueueFree();
    }
}