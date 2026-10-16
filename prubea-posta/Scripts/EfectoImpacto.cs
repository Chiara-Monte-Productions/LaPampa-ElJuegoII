using Godot;

public partial class EfectoImpacto : Node2D
{
    [Export] public float RadioMaximo { get; set; } = 50.0f;
    [Export] public float DuracionExplosion { get; set; } = 0.4f;
    [Export] public float DuracionDesaparicionRastro { get; set; } = 1.5f; // Tiempo que tarda en consumirse el rastro completo
    [Export] public float EsperaAntesDeBorrar { get; set; } = 0.5f;        // Pausa antes de empezar a borrar la estela
    [Export] public Color ColorExplosion { get; set; } = Colors.Orange;

    private Line2D _rastro;
    private float _radioActual = 0.0f;
    private float _opacidadActual = 1.0f;

    public void CrearEfecto(Line2D lineaRastro, Vector2 puntoImpacto)
    {
        GlobalPosition = puntoImpacto;
        _rastro = lineaRastro;

        if (GodotObject.IsInstanceValid(_rastro))
        {
            _rastro.AddPoint(puntoImpacto);

            // Iniciar la eliminación gradual del rastro desde el origen hacia el impacto
            IniciarBorradoGradualRastro();
        }

        // Animación de la explosión circular (crecimiento de 0 a 50px y fade out)
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
        
        // Espera opcional antes de empezar a consumir la estela
        tweenRastro.TweenInterval(EsperaAntesDeBorrar);

        // Anima un índice interpolado desde 0 (origen) hasta el total de puntos (impacto)
        int totalPuntos = _rastro.GetPointCount();
        if (totalPuntos > 0)
        {
            tweenRastro.TweenMethod(Callable.From<int>(RemoverPuntosHasta), 0, totalPuntos, DuracionDesaparicionRastro);
        }

        // Una vez consumida toda la línea, se elimina el nodo de la escena
        tweenRastro.Finished += () =>
        {
            if (GodotObject.IsInstanceValid(_rastro))
            {
                _rastro.QueueFree();
            }
            QueueFree(); // Elimina también el nodo de efecto si ya terminó la explosión
        };
    }

    private void RemoverPuntosHasta(int indice)
    {
        if (!GodotObject.IsInstanceValid(_rastro)) return;

        // Va removiendo iterativamente los puntos iniciales (índice 0)
        while (_rastro.GetPointCount() > (totalPuntosIniciales() - indice) && _rastro.GetPointCount() > 0)
        {
            _rastro.RemovePoint(0);
        }
    }

    private int _puntosIniciales = -1;
    private int totalPuntosIniciales()
    {
        if (_puntosIniciales == -1 && GodotObject.IsInstanceValid(_rastro))
        {
            _puntosIniciales = _rastro.GetPointCount();
        }
        return _puntosIniciales;
    }

    public override void _Draw()
    {
        if (_radioActual > 0)
        {
            Color colorFinal = ColorExplosion;
            colorFinal.A = _opacidadActual;
            DrawCircle(Vector2.Zero, _radioActual, colorFinal);
        }
    }
}