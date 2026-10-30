using Godot;

public partial class Bala : Area2D
{
    [Export] public float Gravedad = 980.0f;
    [Export] public float TiempoDeVida = 5.0f;
    [Export] public float AnchoRastro { get; set; } = 2.0f;
    [Export] public PackedScene EscenaEfectoImpacto { get; set; }

    public ServicioAudio ServicioAudio { get; set; }
    public IServicioViento ServicioViento { get; set; }

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

        if (ServicioAudio == null)
        {
            if (GetTree().GetFirstNodeInGroup("ServicioAudio") is ServicioAudio servicio)
            {
                ServicioAudio = servicio;
            }
        }

        if (ServicioViento == null)
        {
            if (GetTree().GetFirstNodeInGroup("ServicioViento") is IServicioViento viento)
            {
                ServicioViento = viento;
            }
        }

        ServicioAudio?.ReproducirDisparo();
        ServicioAudio?.IniciarSonidoVuelo();

        ElegirColorAleatorio();
        InicializarRastroVisual();
    }

    private void ElegirColorAleatorio()
    {
        if (PaletaColores != null && PaletaColores.Count > 0)
        {
            int indice = GD.RandRange(0, PaletaColores.Count - 1);
            _colorSeleccionado = PaletaColores[indice];
        }
    }

    private void InicializarRastroVisual()
    {
        _lineaRastro = new Line2D
        {
            Width = AnchoRastro,
            DefaultColor = _colorSeleccionado,
            TopLevel = true
        };
        GetParent()?.CallDeferred("add_child", _lineaRastro);
        _lineaRastro.AddPoint(GlobalPosition);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _tiempoEnAire += dt;

        // Aplica la gravedad vertical y la aceleración horizontal del viento
        _velocidadVector.Y += Gravedad * dt;

        if (ServicioViento != null)
        {
            _velocidadVector += ServicioViento.FuerzaVientoActual * dt;
        }

        Position += _velocidadVector * dt;
        Rotation = _velocidadVector.Angle();

        if (GodotObject.IsInstanceValid(_lineaRastro))
        {
            _lineaRastro.AddPoint(GlobalPosition);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        var objetivoDañable = body as DanoProyectil;

        // Evita autocolisión con el tirador en el instante inicial
        if (objetivoDañable != null && _tiempoEnAire < 0.1f) return;

        GenerarEfectoImpacto(GlobalPosition);

        if (objetivoDañable != null)
        {
            ServicioAudio?.ReproducirImpactoJugador();
            objetivoDañable.Destruir();
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

        if (EscenaEfectoImpacto != null && EscenaEfectoImpacto.Instantiate() is EfectoImpacto e)
        {
            efecto = e;
        }
        else
        {
            efecto = new EfectoImpacto();
        }

        efecto.ColorExplosion = _colorSeleccionado;
        GetParent().AddChild(efecto);
        efecto.CrearEfecto(_lineaRastro, puntoImpacto);
    }
}