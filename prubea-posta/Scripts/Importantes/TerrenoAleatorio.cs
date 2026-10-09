using Godot;
using System.Collections.Generic;

public partial class TerrenoAleatorio : StaticBody2D, PosicionTerreno
{
    [ExportGroup("Nodos Hijos")]
    [Export] public Polygon2D PoligonoVisual { get; set; }
    [Export] public CollisionPolygon2D PoligonoColision { get; set; }
    [Export] public Line2D LineaSuperficie { get; set; }

    [ExportGroup("Parámetros del Terreno")]
    [Export] public int AnchoTerreno { get; set; } = 0; // 0 para autoadaptarse al ancho de pantalla
    [Export] public int AlturaBase { get; set; } = 450; // Bajamos un poco la base para dar más espacio a los tanques
    [Export] public int Amplitud { get; set; } = 80;   // Menos altura en los picos
    [Export] public int Resolucion { get; set; } = 60; // Más puntos para que las curvas se vean suaves

    [ExportGroup("Estilo Scorched Earth (Colinas Anchas)")]
    [Export] public float FrecuenciaMontanas { get; set; } = 0.005f; // Controla cuán anchas son las montañas (valores más bajos = colinas más anchas)

    [ExportGroup("Texturas de Terreno")]
    [Export] public Godot.Collections.Array<Texture2D> TexturasTerreno { get; set; } = new();
    [Export] public Vector2 EscalaTextura { get; set; } = new Vector2(100f, 100f);

    private List<Vector2> _puntosTerreno = new List<Vector2>();

    public override void _Ready()
    {
        GenerarTerreno();
    }

    public void GenerarTerreno()
    {
        _puntosTerreno.Clear();

        Vector2 tamanoPantalla = GetViewport().GetVisibleRect().Size;
        float anchoEfectivo = AnchoTerreno > 0 ? AnchoTerreno : tamanoPantalla.X;
        float altoEfectivo = tamanoPantalla.Y + 200f;

        // Semilla o desfase aleatorio para que cada mapa sea diferente
        float desfaseSemilla = (float)GD.RandRange(0f, 1000f);

        // 1. Generar colinas anchas mediante curvas suaves (Seno/Coseno)
        float paso = anchoEfectivo / Resolucion;
        for (int i = 0; i <= Resolucion; i++)
        {
            float x = i * paso;
            
            // Fórmula tipo Scorched Earth: combina ondas suaves + un toque de variación
            float elevacionOnda = Mathf.Sin((x * FrecuenciaMontanas) + desfaseSemilla) * Amplitud;
            float elevacionSecundaria = Mathf.Cos((x * FrecuenciaMontanas * 2.5f) + desfaseSemilla) * (Amplitud * 0.3f);
            
            float y = AlturaBase + elevacionOnda + elevacionSecundaria;
            _puntosTerreno.Add(new Vector2(x, y));
        }

        // 2. Cerrar el polígono hacia el fondo de pantalla
        List<Vector2> puntosPoligono = new List<Vector2>(_puntosTerreno);
        puntosPoligono.Add(new Vector2(anchoEfectivo, altoEfectivo));
        puntosPoligono.Add(new Vector2(0, altoEfectivo));

        Vector2[] arregloPuntos = puntosPoligono.ToArray();
        Texture2D texturaElegida = null;

        // 3. Polygon2D (Visual)
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

        // 5. Línea de Contorno (Line2D)
        if (LineaSuperficie != null)
        {
            LineaSuperficie.Points = _puntosTerreno.ToArray();

            if (texturaElegida != null)
            {
                LineaSuperficie.Texture = texturaElegida;
                LineaSuperficie.TextureMode = Line2D.LineTextureMode.Tile;
            }
        }
    }

    // --- MÉTODOS DE LA INTERFAZ PosicionTerreno ---

    public float ObtenerAlturaEnX(float x)
    {
        if (_puntosTerreno == null || _puntosTerreno.Count < 2) return AlturaBase;

        for (int i = 0; i < _puntosTerreno.Count - 1; i++)
        {
            if (x >= _puntosTerreno[i].X && x <= _puntosTerreno[i + 1].X)
            {
                float t = (x - _puntosTerreno[i].X) / (_puntosTerreno[i + 1].X - _puntosTerreno[i].X);
                return Mathf.Lerp(_puntosTerreno[i].Y, _puntosTerreno[i + 1].Y, t);
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