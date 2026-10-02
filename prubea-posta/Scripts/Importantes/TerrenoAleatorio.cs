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
    [Export] public int AlturaBase { get; set; } = 400;
    [Export] public int Amplitud { get; set; } = 100;
    [Export] public int Resolucion { get; set; } = 30;

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

        // 1. Obtener el ancho real de la pantalla si AnchoTerreno está en 0
        Vector2 tamanoPantalla = GetViewport().GetVisibleRect().Size;
        float anchoEfectivo = AnchoTerreno > 0 ? AnchoTerreno : tamanoPantalla.X;
        float altoEfectivo = tamanoPantalla.Y + 200f; // Asegura cubrir el fondo inferior

        // 2. Generar la curva superior del mapa
        float paso = anchoEfectivo / Resolucion;
        for (int i = 0; i <= Resolucion; i++)
        {
            float x = i * paso;
            float y = AlturaBase + (float)GD.RandRange(-Amplitud, Amplitud);
            _puntosTerreno.Add(new Vector2(x, y));
        }

        // 3. Cerrar el polígono hacia el borde inferior de la pantalla
        List<Vector2> puntosPoligono = new List<Vector2>(_puntosTerreno);
        puntosPoligono.Add(new Vector2(anchoEfectivo, altoEfectivo)); // Esquina inferior derecha
        puntosPoligono.Add(new Vector2(0, altoEfectivo));            // Esquina inferior izquierda

        Vector2[] arregloPuntos = puntosPoligono.ToArray();

        Texture2D texturaElegida = null;

        // 4. Asignar geometría y textura aleatoria al Polygon2D visual
        if (PoligonoVisual != null)
        {
            PoligonoVisual.Polygon = arregloPuntos;

            if (TexturasTerreno != null && TexturasTerreno.Count > 0)
            {
                // Seleccionar textura aleatoria del arreglo
                int indiceAleatorio = GD.RandRange(0, TexturasTerreno.Count - 1);
                texturaElegida = TexturasTerreno[indiceAleatorio];

                PoligonoVisual.Texture = texturaElegida;
                PoligonoVisual.TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;

                // Coordenadas UV para evitar que la imagen se deforme
                Vector2[] uvs = new Vector2[arregloPuntos.Length];
                for (int i = 0; i < arregloPuntos.Length; i++)
                {
                    uvs[i] = new Vector2(arregloPuntos[i].X / EscalaTextura.X, arregloPuntos[i].Y / EscalaTextura.Y);
                }

                PoligonoVisual.UV = uvs;
            }
        }

        // 5. Asignar la colisión física
        if (PoligonoColision != null)
        {
            PoligonoColision.Polygon = arregloPuntos;
        }

        // 6. Asignar el borde superior en Line2D y aplicar textura si existe
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