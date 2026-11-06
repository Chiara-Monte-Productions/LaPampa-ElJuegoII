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

        ServicioAudio = GetTree().GetFirstNodeInGroup("ServicioAudio") as ServicioAudio;
        ServicioViento = GetTree().GetFirstNodeInGroup("ServicioViento") as IServicioViento;

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
        if (_tiempoEnAire < 0.1f) return;

        GenerarEfectoImpacto(GlobalPosition);

        // Polimorfismo puro sin evaluar condicional de tipo 'is'
        DanoProyectil objetivoDanable = body as DanoProyectil;
        ProcesarImpacto(objetivoDanable);

        QueueFree();
    }

    private void ProcesarImpacto(DanoProyectil objetivo)
    {
        if (objetivo != null)
        {
            ServicioAudio?.ReproducirImpactoJugador();
            objetivo.Destruir();
        }
        else
        {
            ServicioAudio?.ReproducirImpactoTerreno();
        }
    }

    private void OnTiempoExpirado()
    {
        ServicioAudio?.DetenerSonidoVuelo();
        QueueFree();
    }

    private void GenerarEfectoImpacto(Vector2 puntoImpacto)
    {
        EfectoImpacto efecto = EscenaEfectoImpacto != null 
            ? EscenaEfectoImpacto.Instantiate() as EfectoImpacto 
            : new EfectoImpacto();

        if (efecto != null)
        {
            efecto.ColorExplosion = _colorSeleccionado;
            GetParent().AddChild(efecto);
            efecto.CrearEfecto(_lineaRastro, puntoImpacto);
        }
    }
}