using Godot;

public partial class Player : CharacterBody2D, Angulo, DanoProyectil
{
    [Signal] public delegate void DisparoRealizadoEventHandler(Bala bala);
    [Signal] public delegate void JugadorMuertoEventHandler(Player jugador);

    [Export] public PackedScene EscenaBala { get; set; }
    [Export] public Node2D PuntoDeDisparo { get; set; }

    [Export] public float FuerzaDisparo { get; private set; } = 600.0f;
    [Export] public float FuerzaMinima { get; set; } = 200.0f;
    [Export] public float FuerzaMaxima { get; set; } = 1200.0f;

    [Export] public float VelocidadRotacion { get; set; } = 2.0f;
    [Export] public float VelocidadCargaFuerza { get; set; } = 400.0f;

    public bool EsMiTurno { get; set; } = false;

    public float ObtenerAngulo()
    {
        if (PuntoDeDisparo == null) return 0f;
        return Mathf.Round(-PuntoDeDisparo.RotationDegrees);
    }

    public override void _Process(double delta)
    {
        if (!EsMiTurno) return;

        float dt = (float)delta;

        if (PuntoDeDisparo != null)
        {
            if (Input.IsActionPressed("ui_up")) PuntoDeDisparo.Rotate(-VelocidadRotacion * dt);
            if (Input.IsActionPressed("ui_down")) PuntoDeDisparo.Rotate(VelocidadRotacion * dt);
        }

        if (Input.IsActionPressed("ui_right"))
            FuerzaDisparo = Mathf.Min(FuerzaDisparo + VelocidadCargaFuerza * dt, FuerzaMaxima);

        if (Input.IsActionPressed("ui_left"))
            FuerzaDisparo = Mathf.Max(FuerzaDisparo - VelocidadCargaFuerza * dt, FuerzaMinima);

        if (Input.IsActionJustPressed("ui_accept"))
        {
            Disparar();
        }
    }

    private void Disparar()
    {
        if (EscenaBala == null) return;

        Vector2 posOrigen = PuntoDeDisparo != null ? PuntoDeDisparo.GlobalPosition : GlobalPosition;
        float rotOrigen = PuntoDeDisparo != null ? PuntoDeDisparo.GlobalRotation : GlobalRotation;

        Node instancia = EscenaBala.Instantiate();

        if (instancia is Bala bala)
        {
            bala.GlobalPosition = posOrigen;
            bala.GlobalRotation = rotOrigen;
            bala.Inicializar(FuerzaDisparo);

            GetParent().AddChild(bala);

            EsMiTurno = false;
            EmitSignal(SignalName.DisparoRealizado, bala);
        }
    }

    public void Destruir()
    {
        EmitSignal(SignalName.JugadorMuerto, this);
        QueueFree();
    }
}