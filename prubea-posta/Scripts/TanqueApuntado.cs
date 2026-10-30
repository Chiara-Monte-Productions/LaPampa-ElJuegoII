using Godot;

public partial class TanqueApuntado : Node2D
{
    [ExportGroup("Referencias")]
    [Export] public Marker2D PivoteCanon { get; set; }

    [ExportGroup("Límites de Disparo")]
    [Export] public float FuerzaMinima { get; set; } = 100f;
    [Export] public float FuerzaMaxima { get; set; } = 1000f;

    [ExportGroup("Configuración de Espera")]
    [Export] public float TiempoEsperaNormal { get; set; } = 0.15f;

    public bool EsMiTurno { get; set; } = false;
    public float AnguloActual { get; private set; } = 45f;
    public float FuerzaActual { get; private set; } = 500f;

    private float _timerAngulo = 0f;
    private float _timerFuerza = 0f;

    public override void _Ready()
    {
        ActualizarVisuales();
    }

    public override void _Process(double delta)
    {
        if (!EsMiTurno) return;

        float dt = (float)delta;
        ActualizarCooldowns(dt);

        bool modoRapido = Input.IsActionPressed("modo_rapido") || Input.IsKeyPressed(Key.Shift);
        float cooldownRequerido = modoRapido ? 0f : TiempoEsperaNormal;

        ProcesarControlAngulo(modoRapido ? 1f : 1f, cooldownRequerido);
        ProcesarControlFuerza(modoRapido ? 10f : 5f, cooldownRequerido);
    }

    private void ActualizarCooldowns(float dt)
    {
        _timerAngulo -= dt;
        _timerFuerza -= dt;
    }

    private void ProcesarControlAngulo(float paso, float cooldown)
    {
        if (_timerAngulo > 0f) return;

        if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W))
        {
            AnguloActual = (AnguloActual + paso > 180f) ? 0f : AnguloActual + paso;
            _timerAngulo = cooldown;
            ActualizarVisuales();
        }
        else if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S))
        {
            AnguloActual = (AnguloActual - paso < 0f) ? 180f : AnguloActual - paso;
            _timerAngulo = cooldown;
            ActualizarVisuales();
        }
    }

    private void ProcesarControlFuerza(float paso, float cooldown)
    {
        if (_timerFuerza > 0f) return;

        if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D))
        {
            FuerzaActual = Mathf.Clamp(FuerzaActual + paso, FuerzaMinima, FuerzaMaxima);
            _timerFuerza = cooldown;
        }
        else if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A))
        {
            FuerzaActual = Mathf.Clamp(FuerzaActual - paso, FuerzaMinima, FuerzaMaxima);
            _timerFuerza = cooldown;
        }
    }

    public void ActualizarVisuales()
    {
        if (PivoteCanon != null)
        {
            PivoteCanon.RotationDegrees = -AnguloActual;
        }
    }
}