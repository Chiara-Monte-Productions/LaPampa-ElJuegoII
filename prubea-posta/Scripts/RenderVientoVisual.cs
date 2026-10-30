using Godot;

public partial class RenderVientoVisual : Node2D
{
    [ExportGroup("Configuración de Partículas")]
    [Export] public int CantidadLineas { get; set; } = 35;
    [Export] public float LargoLinea { get; set; } = 40.0f;
    [Export] public float AnchoLinea { get; set; } = 1.5f;
    [Export] public Color ColorViento { get; set; } = new Color(1.0f, 1.0f, 1.0f, 0.25f); // Blanco transparente

    [ExportGroup("Sensibilidad de Velocidad")]
    [Export] public float FactorVelocidad { get; set; } = 1.2f;

    public IServicioViento ServicioViento { get; set; }

    private float[] _posicionesX;
    private float[] _posicionesY;
    private float[] _velocidadesRelativas;
    private Vector2 _tamanoPantalla;

    public override void _Ready()
    {
        _tamanoPantalla = GetViewport().GetVisibleRect().Size;

        // Intentar obtener el servicio de viento por grupo (DIP)
        if (ServicioViento == null)
        {
            if (GetTree().GetFirstNodeInGroup("ServicioViento") is IServicioViento viento)
            {
                ServicioViento = viento;
            }
        }

        InicializarParticulas();
    }

    private void InicializarParticulas()
    {
        _posicionesX = new float[CantidadLineas];
        _posicionesY = new float[CantidadLineas];
        _velocidadesRelativas = new float[CantidadLineas];

        for (int i = 0; i < CantidadLineas; i++)
        {
            _posicionesX[i] = (float)GD.RandRange(0, _tamanoPantalla.X);
            _posicionesY[i] = (float)GD.RandRange(20, _tamanoPantalla.Y - 100); // Evitar el piso
            _velocidadesRelativas[i] = (float)GD.RandRange(0.8f, 1.3f); // Variación de velocidad individual
        }
    }

    public override void _Process(double delta)
    {
        if (ServicioViento == null) return;

        float fuerzaX = ServicioViento.FuerzaVientoActual.X;
        
        // Si no hay viento, no se mueven las líneas
        if (Mathf.Abs(fuerzaX) < 1.0f) return;

        float dt = (float)delta;
        float velocidadBase = fuerzaX * FactorVelocidad;

        for (int i = 0; i < CantidadLineas; i++)
        {
            // Mover según la dirección y magnitud del viento
            _posicionesX[i] += velocidadBase * _velocidadesRelativas[i] * dt;

            // Bucle infinito: al salir de un lado de la pantalla, reaparecer por el opuesto
            if (_posicionesX[i] > _tamanoPantalla.X + LargoLinea)
            {
                _posicionesX[i] = -LargoLinea;
                _posicionesY[i] = (float)GD.RandRange(20, _tamanoPantalla.Y - 100);
            }
            else if (_posicionesX[i] < -LargoLinea)
            {
                _posicionesX[i] = _tamanoPantalla.X + LargoLinea;
                _posicionesY[i] = (float)GD.RandRange(20, _tamanoPantalla.Y - 100);
            }
        }

        // Redibujar en cada frame
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (ServicioViento == null) return;

        float fuerzaX = ServicioViento.FuerzaVientoActual.X;
        if (Mathf.Abs(fuerzaX) < 1.0f) return;

        // La dirección del viento determina hacia dónde apunta la cola/cabeza de la línea
        float direccion = Mathf.Sign(fuerzaX);
        float largoEfectivo = LargoLinea * direccion;

        for (int i = 0; i < CantidadLineas; i++)
        {
            Vector2 inicio = new Vector2(_posicionesX[i], _posicionesY[i]);
            Vector2 fin = new Vector2(_posicionesX[i] + largoEfectivo, _posicionesY[i]);

            DrawLine(inicio, fin, ColorViento, AnchoLinea);
        }
    }
}