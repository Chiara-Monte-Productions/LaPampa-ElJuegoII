using Godot;
using System.Collections.Generic;

public abstract partial class BaseGeneradorTerreno : StaticBody2D, PosicionTerreno
{
    [ExportGroup("Nodos Hijos")]
    [Export] public Polygon2D PoligonoVisual { get; set; }
    [Export] public CollisionPolygon2D PoligonoColision { get; set; }
    [Export] public Line2D LineaSuperficie { get; set; }

    [ExportGroup("Parámetros del Terreno")]
    [Export] public int AnchoTerreno { get; set; } = 0;
    [Export] public int AlturaBase { get; set; } = 450;
    [Export] public int Amplitud { get; set; } = 80;
    [Export] public int Resolucion { get; set; } = 60;

    [ExportGroup("Estilo Scorched Earth (Colinas Anchas)")]
    [Export] public float FrecuenciaMontanas { get; set; } = 0.005f;

    [ExportGroup("Texturas de Terreno")]
    [Export] public Godot.Collections.Array<Texture2D> TexturasTerreno { get; set; } = new();
    [Export] public Vector2 EscalaTextura { get; set; } = new Vector2(100f, 100f);

    public List<Vector2> PuntosTerreno { get; protected set; } = new();

    public override void _Ready()
    {
        GenerarTerreno();
    }

    public void GenerarTerreno()
    {
        PuntosTerreno.Clear();

        Vector2 tamanoPantalla = GetViewport().GetVisibleRect().Size;
        float anchoEfectivo = AnchoTerreno > 0 ? AnchoTerreno : tamanoPantalla.X;
        float altoEfectivo = tamanoPantalla.Y + 200f;

        CalcularPuntosMatematicos(anchoEfectivo);

        Vector2[] arregloPuntos = ConstruirPuntosPoligono(anchoEfectivo, altoEfectivo);
        Texture2D texturaElegida = ObtenerTexturaAleatoria();

        AplicarRenderVisual(arregloPuntos, texturaElegida);
        AplicarColisionFisica(arregloPuntos);
        AplicarContornoSuperficie(texturaElegida);
    }

    private void CalcularPuntosMatematicos(float anchoEfectivo)
    {
        float desfaseSemilla = (float)GD.RandRange(0f, 1000f);
        float paso = anchoEfectivo / Resolucion;

        for (int i = 0; i <= Resolucion; i++)
        {
            float x = i * paso;
            float elevacionOnda = Mathf.Sin((x * FrecuenciaMontanas) + desfaseSemilla) * Amplitud;
            float elevacionSecundaria = Mathf.Cos((x * FrecuenciaMontanas * 2.5f) + desfaseSemilla) * (Amplitud * 0.3f);
            
            float y = AlturaBase + elevacionOnda + elevacionSecundaria;
            PuntosTerreno.Add(new Vector2(x, y));
        }
    }

    private Vector2[] ConstruirPuntosPoligono(float anchoEfectivo, float altoEfectivo)
    {
        List<Vector2> puntosPoligono = new(PuntosTerreno)
        {
            new Vector2(anchoEfectivo, altoEfectivo),
            new Vector2(0, altoEfectivo)
        };
        return puntosPoligono.ToArray();
    }

    private Texture2D ObtenerTexturaAleatoria()
    {
        if (TexturasTerreno != null && TexturasTerreno.Count > 0)
        {
            int indiceAleatorio = GD.RandRange(0, TexturasTerreno.Count - 1);
            return TexturasTerreno[indiceAleatorio];
        }
        return null;
    }

    private void AplicarRenderVisual(Vector2[] arregloPuntos, Texture2D texturaElegida)
    {
        if (PoligonoVisual == null) return;

        PoligonoVisual.Polygon = arregloPuntos;

        if (texturaElegida != null)
        {
            PoligonoVisual.Texture = texturaElegida;
            PoligonoVisual.TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;

            Vector2[] uvs = new Vector2[arregloPuntos.Length];
            for (int i = 0; i < arregloPuntos.Length; i++)
            {
                uvs[i] = new Vector2(arregloPuntos[i].X / EscalaTextura.X, arregloPuntos[i].Y / EscalaTextura.Y);
            }

            PoligonoVisual.UV = uvs;
        }
    }

    private void AplicarColisionFisica(Vector2[] arregloPuntos)
    {
        if (PoligonoColision != null)
        {
            PoligonoColision.Polygon = arregloPuntos;
        }
    }

    private void AplicarContornoSuperficie(Texture2D texturaElegida)
    {
        if (LineaSuperficie == null) return;

        LineaSuperficie.Points = PuntosTerreno.ToArray();

        if (texturaElegida != null)
        {
            LineaSuperficie.Texture = texturaElegida;
            LineaSuperficie.TextureMode = Line2D.LineTextureMode.Tile;
        }
    }

    // --- CONTRATO DE INTERFAZ PosicionTerreno ---

    public float ObtenerAlturaEnX(float x)
    {
        if (PuntosTerreno == null || PuntosTerreno.Count < 2) return AlturaBase;

        for (int i = 0; i < PuntosTerreno.Count - 1; i++)
        {
            if (x >= PuntosTerreno[i].X && x <= PuntosTerreno[i + 1].X)
            {
                float t = (x - PuntosTerreno[i].X) / (PuntosTerreno[i + 1].X - PuntosTerreno[i].X);
                return Mathf.Lerp(PuntosTerreno[i].Y, PuntosTerreno[i + 1].Y, t);
            }
        }

        return AlturaBase;
    }

    public void AjustarObjetoAlPiso(Node2D objeto, float x)
    {
        if (objeto == null) return;

        float alturaY = ObtenerAlturaEnX(x);
        objeto.GlobalPosition = new Vector2(x, alturaY);
    }
}