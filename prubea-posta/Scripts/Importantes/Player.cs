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
            // Sincroniza el estado del turno con el componente de apuntado
            if (ComponenteApuntado != null)
            {
                ComponenteApuntado.EsMiTurno = value;
            }
        }
    }

    // --- IMPLEMENTACIÓN DE LA INTERFAZ 'Angulo' ---

    public float ObtenerAngulo()
    {
        return ComponenteApuntado != null ? ComponenteApuntado.AnguloActual : 0f;
    }

    public float FuerzaDisparo
    {
        get => ComponenteApuntado != null ? ComponenteApuntado.FuerzaActual : 0f;
    }

    // ----------------------------------------------

    public override void _Ready()
    {
        // Aseguramos sincronización inicial al instanciar el objeto
        if (ComponenteApuntado != null)
        {
            ComponenteApuntado.EsMiTurno = EsMiTurno;
        }
    }

    public override void _Process(double delta)
    {
        if (!EsMiTurno) return;

        if (Input.IsActionJustPressed("ui_accept"))
        {
            Disparar();
        }
    }

    private void Disparar()
    {
        if (EscenaBala == null || ComponenteApuntado == null) return;

        Vector2 posOrigen = ComponenteApuntado.PivoteCanon != null 
            ? ComponenteApuntado.PivoteCanon.GlobalPosition 
            : GlobalPosition;

        float rotOrigen = ComponenteApuntado.PivoteCanon != null 
            ? ComponenteApuntado.PivoteCanon.GlobalRotation 
            : GlobalRotation;

        Node instancia = EscenaBala.Instantiate();

        if (instancia is Bala bala)
        {
            bala.GlobalPosition = posOrigen;
            bala.GlobalRotation = rotOrigen;
            bala.Inicializar(FuerzaDisparo);

            GetParent().AddChild(bala);

            EsMiTurno = false; // Al quitar el turno se deshabilita la entrada del componente
            EmitSignal(SignalName.DisparoRealizado, bala);
        }
    }

    public void Destruir()
    {
        EmitSignal(SignalName.JugadorMuerto, this);
        QueueFree();
    }
}