using Godot;
using System.Collections.Generic;

public partial class GeneradorTerreno : StaticBody2D
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

    [ExportGroup("Estilo Scorched Earth")]
    [Export] public float FrecuenciaMontanas { get; set; } = 0.005f;

    [ExportGroup("Texturas de Terreno")]
    [Export] public Godot.Collections.Array<Texture2D> TexturasTerreno { get; set; } = new();
    [Export] public Vector2 EscalaTextura { get; set; } = new Vector2(100f, 100f);

    public List<Vector2> PuntosTerreno { get; private set; } = new List<Vector2>();

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

        float desfaseSemilla = (float)GD.RandRange(0f, 1000f);

        // 1. Generar curvas suaves
        float paso = anchoEfectivo / Resolucion;
        for (int i = 0; i <= Resolucion; i++)
        {
            float x = i * paso;
            float elevacionOnda = Mathf.Sin((x * FrecuenciaMontanas) + desfaseSemilla) * Amplitud;
            float elevacionSecundaria = Mathf.Cos((x * FrecuenciaMontanas * 2.5f) + desfaseSemilla) * (Amplitud * 0.3f);
            
            float y = AlturaBase + elevacionOnda + elevacionSecundaria;
            PuntosTerreno.Add(new Vector2(x, y));
        }

        // 2. Construir la geometría
        List<Vector2> puntosPoligono = new List<Vector2>(PuntosTerreno);
        puntosPoligono.Add(new Vector2(anchoEfectivo, altoEfectivo));
        puntosPoligono.Add(new Vector2(0, altoEfectivo));

        Vector2[] arregloPuntos = puntosPoligono.ToArray();
        Texture2D texturaElegida = null;

        // 3. Renderizado Visual
        if (PoligonoVisual != null)
        {
            PoligonoVisual.Polygon = arregloPuntos;

            if (TexturasTerreno != null && TexturasTerreno.Count > 0)
            {
                int indiceAleatorio = GD.RandRange(0, TexturasTerreno.Count - 1);
                texturaElegida = TexturasTerreno[indiceAleatorio];

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

        // 4. Colisión Física
        if (PoligonoColision != null)
        {
            PoligonoColision.Polygon = arregloPuntos;
        }

        // 5. Línea de Contorno
        if (LineaSuperficie != null)
        {
            LineaSuperficie.Points = PuntosTerreno.ToArray();

            if (texturaElegida != null)
            {
                LineaSuperficie.Texture = texturaElegida;
                LineaSuperficie.TextureMode = Line2D.LineTextureMode.Tile;
            }
        }
    }
}
