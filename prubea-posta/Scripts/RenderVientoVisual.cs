using Godot;

public partial class RenderVientoVisual : Node2D
{
    [ExportGroup("Configuración de Partículas")]
    [Export] public int CantidadLineas { get; set; } = 35;
    [Export] public float LargoLinea { get; set; } = 40.0f;
    [Export] public float AnchoLinea { get; set; } = 1.5f;
    [Export] public Color ColorViento { get; set; } = new Color(1.0f, 1.0f, 1.0f, 0.25f);

    [ExportGroup("Sensibilidad de Velocidad")]
    [Export] public float FactorVelocidad { get; set; } = 1.2f;

    // DIP: Dependemos de la abstracción IServicioViento, no de una clase concreta
    public IServicioViento ServicioViento { get; set; }

    private float[] _posicionesX;
    private float[] _posicionesY;
    private float[] _velocidadesRelativas;
    private Vector2 _tamanoPantalla;

    public override void _Ready()
    {
        _tamanoPantalla = GetViewport().GetVisibleRect().Size;
        ObtenerServicioViento();
        InicializarParticulas();
    }

    private void ObtenerServicioViento()
    {
        if (ServicioViento == null)
        {
            if (GetTree().GetFirstNodeInGroup("ServicioViento") is IServicioViento viento)
            {
                ServicioViento = viento;
            }
        }
    }

    private void InicializarParticulas()
    {
        _posicionesX = new float[CantidadLineas];
        _posicionesY = new float[CantidadLineas];
        _velocidadesRelativas = new float[CantidadLineas];

        for (int i = 0; i < CantidadLineas; i++)
        {
            _posicionesX[i] = (float)GD.RandRange(0, _tamanoPantalla.X);
            _posicionesY[i] = (float)GD.RandRange(20, _tamanoPantalla.Y - 100);
            _velocidadesRelativas[i] = (float)GD.RandRange(0.8f, 1.3f);
        }
    }

    public override void _Process(double delta)
    {
        if (ServicioViento == null)
        {
            ObtenerServicioViento();
            if (ServicioViento == null) return;
        }

        float fuerzaX = ServicioViento.FuerzaVientoActual.X;
        if (Mathf.Abs(fuerzaX) < 1.0f) return;

        ActualizarPosiciones((float)delta, fuerzaX);
        QueueRedraw();
    }

    private void ActualizarPosiciones(float dt, float fuerzaX)
    {
        float velocidadBase = fuerzaX * FactorVelocidad;

        for (int i = 0; i < CantidadLineas; i++)
        {
            _posicionesX[i] += velocidadBase * _velocidadesRelativas[i] * dt;

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
    }

    public override void _Draw()
    {
        if (ServicioViento == null) return;

        float fuerzaX = ServicioViento.FuerzaVientoActual.X;
        if (Mathf.Abs(fuerzaX) < 1.0f) return;

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