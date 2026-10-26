using Godot;

public partial class EfectoImpacto : Node2D
{
    [Export] public float RadioMaximo { get; set; } = 50.0f;
    [Export] public float DuracionExplosion { get; set; } = 0.4f;
    [Export] public float DuracionDesaparicionRastro { get; set; } = 1.5f;
    [Export] public float EsperaAntesDeBorrar { get; set; } = 0.5f;
    [Export] public Color ColorExplosion { get; set; } = Colors.Orange;

    private Line2D _rastro;
    private float _radioActual = 0.0f;
    private float _opacidadActual = 1.0f;
    private int _puntosIniciales = -1;

    public void CrearEfecto(Line2D lineaRastro, Vector2 puntoImpacto)
    {
        GlobalPosition = puntoImpacto;
        _rastro = lineaRastro;

        if (GodotObject.IsInstanceValid(_rastro))
        {
            _rastro.AddPoint(puntoImpacto);
            IniciarBorradoGradualRastro();
        }

        // Animación de expansión y desvanecimiento de la explosión
        Tween tweenExplosion = CreateTween();
        tweenExplosion.SetParallel(true);

        tweenExplosion.TweenMethod(Callable.From<float>(r =>
        {
            _radioActual = r;
            QueueRedraw();
        }), 0.0f, RadioMaximo, DuracionExplosion);

        tweenExplosion.TweenMethod(Callable.From<float>(a =>
        {
            _opacidadActual = a;
            QueueRedraw();
        }), 1.0f, 0.0f, DuracionExplosion);
    }

    private void IniciarBorradoGradualRastro()
    {
        Tween tweenRastro = CreateTween();
        tweenRastro.TweenInterval(EsperaAntesDeBorrar);

        if (GodotObject.IsInstanceValid(_rastro) && _rastro.GetPointCount() > 0)
        {
            int totalPuntos = _rastro.GetPointCount();
            tweenRastro.TweenMethod(Callable.From<int>(RemoverPuntosHasta), 0, totalPuntos, DuracionDesaparicionRastro);
        }

        tweenRastro.Finished += () =>
        {
            if (GodotObject.IsInstanceValid(_rastro))
            {
                _rastro.QueueFree();
            }
            QueueFree();
        };
    }

    private void RemoverPuntosHasta(int indice)
    {
        if (!GodotObject.IsInstanceValid(_rastro)) return;

        int totalInicial = ObtenerPuntosIniciales();
        while (_rastro.GetPointCount() > (totalInicial - indice) && _rastro.GetPointCount() > 0)
        {
            _rastro.RemovePoint(0);
        }
    }

    private int ObtenerPuntosIniciales()
    {
        if (_puntosIniciales == -1 && GodotObject.IsInstanceValid(_rastro))
        {
            _puntosIniciales = _rastro.GetPointCount();
        }
        return _puntosIniciales;
    }

    public override void _Draw()
    {
        if (_radioActual > 0.1f)
        {
            Color colorFinal = ColorExplosion;
            colorFinal.A = _opacidadActual;
            DrawCircle(Vector2.Zero, _radioActual, colorFinal);
        }
    }
}