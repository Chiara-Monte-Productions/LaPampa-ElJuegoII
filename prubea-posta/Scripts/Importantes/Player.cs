using Godot;

public partial class Player : CharacterBody2D, Angulo, DanoProyectil
{
    [Signal] public delegate void DisparoRealizadoEventHandler(Bala bala);
    [Signal] public delegate void JugadorMuertoEventHandler(Player jugador);

    [ExportGroup("Componentes y Referencias")]
    [Export] public PackedScene EscenaBala { get; set; }
    [Export] public TanqueApuntado ComponenteApuntado { get; set; } 

    private bool _esMiTurno = false;

    [ExportGroup("Estado de Turno")]
    [Export] 
    public bool EsMiTurno 
    { 
        get => _esMiTurno;
        set
        {
            _esMiTurno = value;
            if (ComponenteApuntado != null)
            {
                ComponenteApuntado.EsMiTurno = value;
            }
        }
    }

    // --- IMPLEMENTACIÓN DE INTERFAZ Angulo ---

    public float ObtenerAngulo()
    {
        return ComponenteApuntado != null ? ComponenteApuntado.AnguloActual : 0f;
    }

    public float FuerzaDisparo
    {
        get => ComponenteApuntado != null ? ComponenteApuntado.FuerzaActual : 0f;
    }

    // ----------------------------------------

    public override void _Ready()
    {
        if (ComponenteApuntado != null)
        {
            ComponenteApuntado.EsMiTurno = EsMiTurno;
        }
    }

    public override void _Process(double delta)
    {
        if (!_esMiTurno) return;

        if (Input.IsActionJustPressed("ui_accept"))
        {
            Disparar();
        }
    }

    private void Disparar()
    {
        if (EscenaBala == null || ComponenteApuntado == null) return;

        if (EscenaBala.Instantiate() is Bala bala)
        {
            bala.GlobalPosition = ObtenerPosicionOrigenDisparo();
            bala.GlobalRotation = ObtenerRotacionOrigenDisparo();
            bala.Inicializar(FuerzaDisparo);

            GetParent().AddChild(bala);

            EsMiTurno = false;
            EmitSignal(SignalName.DisparoRealizado, bala);
        }
    }

    private Vector2 ObtenerPosicionOrigenDisparo()
    {
        return ComponenteApuntado.PivoteCanon != null 
            ? ComponenteApuntado.PivoteCanon.GlobalPosition 
            : GlobalPosition;
    }

    private float ObtenerRotacionOrigenDisparo()
    {
        return ComponenteApuntado.PivoteCanon != null 
            ? ComponenteApuntado.PivoteCanon.GlobalRotation 
            : GlobalRotation;
    }

    // --- IMPLEMENTACIÓN DE INTERFAZ DanoProyectil ---

    public void Destruir()
    {
        EmitSignal(SignalName.JugadorMuerto, this);
        QueueFree();
    }
}