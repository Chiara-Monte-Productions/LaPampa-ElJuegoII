using Godot;

public partial class Bala : Area2D
{
    [Export] public float Gravedad = 980.0f;
    [Export] public float TiempoDeVida = 5.0f;
    [Export] public float AnchoRastro { get; set; } = 2.0f;
    [Export] public PackedScene EscenaEfectoImpacto { get; set; }

    // Abstracción inyectada o resuelta localmente (DIP)
    public IServicioAudio ServicioAudio { get; set; }

    [Export] 
    public Godot.Collections.Array<Color> PaletaColores { get; set; } = new()
    {
        Colors.Cyan, Colors.Yellow, Colors.Magenta,
        Colors.Orange, Colors.Lime, Colors.Coral, Colors.LightSkyBlue
    };

    private Vector2 _velocidadVector;
    private float _tiempoEnAire = 0.0f;
    private Line2D _lineaRastro;
    private Color _colorSeleccionado = Colors.White;

    public void Inicializar(float fuerza)
    {
        _velocidadVector = Transform.X * fuerza;
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        GetTree().CreateTimer(TiempoDeVida).Timeout += OnTiempoExpirado;

        // Resolución del servicio en el árbol de la escena local si no fue inyectado
        if (ServicioAudio == null)
        {
            ServicioAudio = GetTree().GetFirstNodeInGroup("ServicioAudio") as IServicioAudio;
        }

        // Notificación de eventos de ciclo de vida
        ServicioAudio?.ReproducirDisparo();
        ServicioAudio?.IniciarSonidoVuelo();

        ElegirColorAleatorio();

        _lineaRastro = new Line2D
        {
            Width = AnchoRastro,
            DefaultColor = _colorSeleccionado,
            TopLevel = true
        };
        
        GetParent()?.CallDeferred("add_child", _lineaRastro);
        _lineaRastro.AddPoint(GlobalPosition);
    }

    private void ElegirColorAleatorio()
    {
        if (PaletaColores != null && PaletaColores.Count > 0)
        {
            int indice = GD.RandRange(0, PaletaColores.Count - 1);
            _colorSeleccionado = PaletaColores[indice];
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _tiempoEnAire += dt;

        _velocidadVector.Y += Gravedad * dt;
        Position += _velocidadVector * dt;
        Rotation = _velocidadVector.Angle();

        if (GodotObject.IsInstanceValid(_lineaRastro))
        {
            _lineaRastro.AddPoint(GlobalPosition);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is DanoProyectil && _tiempoEnAire < 0.1f) return;

        GenerarEfectoImpacto(GlobalPosition);

        if (body is DanoProyectil entidadDestruible)
        {
            ServicioAudio?.ReproducirImpactoJugador();
            GD.Print("¡Objetivo destruido!");
            entidadDestruible.Destruir();
        }
        else
        {
            ServicioAudio?.ReproducirImpactoTerreno();
        }

        QueueFree();
    }

    private void OnTiempoExpirado()
    {
        ServicioAudio?.DetenerSonidoVuelo();
        QueueFree();
    }

    private void GenerarEfectoImpacto(Vector2 puntoImpacto)
    {
        EfectoImpacto efecto = null;

        if (EscenaEfectoImpacto != null)
        {
            Node instancia = EscenaEfectoImpacto.Instantiate();
            if (instancia is EfectoImpacto e) efecto = e;
        }

        if (efecto == null)
        {
            efecto = new EfectoImpacto();
        }

        efecto.ColorExplosion = _colorSeleccionado;
        GetParent().AddChild(efecto);
        efecto.CrearEfecto(_lineaRastro, puntoImpacto);
    }

    
}