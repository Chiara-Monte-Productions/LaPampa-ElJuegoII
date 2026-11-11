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

    public void CrearEfecto(Line2D lineaRastro, Vector2 puntoImpacto)
    {
        GlobalPosition = puntoImpacto;
        _rastro = lineaRastro;

        if (_rastro != null)
        {
            _rastro.AddPoint(puntoImpacto);
            IniciarBorradoGradualRastro();
        }

        Tween tweenExplosion = CreateTween();
        tweenExplosion.SetParallel(true);

        tweenExplosion.TweenMethod(Callable.From<float>(AnimarRadio), 0.0f, RadioMaximo, DuracionExplosion);
        tweenExplosion.TweenMethod(Callable.From<float>(AnimarOpacidad), 1.0f, 0.0f, DuracionExplosion);
    }

    private void AnimarRadio(float r)
    {
        _radioActual = r;
        QueueRedraw();
    }

    private void AnimarOpacidad(float a)
    {
        _opacidadActual = a;
        QueueRedraw();
    }

    private void IniciarBorradoGradualRastro()
    {
        Tween tweenRastro = CreateTween();
        tweenRastro.TweenInterval(EsperaAntesDeBorrar);

        tweenRastro.Finished += () =>
        {
            _rastro?.QueueFree();
            QueueFree();
        };
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