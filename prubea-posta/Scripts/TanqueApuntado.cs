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

    // Estado del turno manejado por Player
    public bool EsMiTurno { get; set; } = false;

    // Cada instancia de este script mantiene sus propios valores guardados
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
        // SI NO ES EL TURNO DE ESTE TANQUE, NO SE PROCESA EL TECLADO
        if (!EsMiTurno) return;

        float dt = (float)delta;

        _timerAngulo -= dt;
        _timerFuerza -= dt;

        bool modoRapido = Input.IsActionPressed("modo_rapido") || Input.IsKeyPressed(Key.Shift);

        float pasoAngulo = modoRapido ? 1f : 1f;
        float pasoFuerza = modoRapido ? 10f : 5f;
        float cooldownRequerido = modoRapido ? 0f : TiempoEsperaNormal;

        // --- CONTROL DE ÁNGULO (0° a 180°) ---
        if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W))
        {
            if (_timerAngulo <= 0f)
            {
                AnguloActual += pasoAngulo;
                if (AnguloActual > 180f) AnguloActual = 0f;

                _timerAngulo = cooldownRequerido;
                ActualizarVisuales();
            }
        }
        else if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S))
        {
            if (_timerAngulo <= 0f)
            {
                AnguloActual -= pasoAngulo;
                if (AnguloActual < 0f) AnguloActual = 180f;

                _timerAngulo = cooldownRequerido;
                ActualizarVisuales();
            }
        }

        // --- CONTROL DE FUERZA ---
        if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D))
        {
            if (_timerFuerza <= 0f)
            {
                FuerzaActual = Mathf.Clamp(FuerzaActual + pasoFuerza, FuerzaMinima, FuerzaMaxima);
                _timerFuerza = cooldownRequerido;
            }
        }
        else if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A))
        {
            if (_timerFuerza <= 0f)
            {
                FuerzaActual = Mathf.Clamp(FuerzaActual - pasoFuerza, FuerzaMinima, FuerzaMaxima);
                _timerFuerza = cooldownRequerido;
            }
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