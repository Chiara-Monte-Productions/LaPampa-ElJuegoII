using Godot;

public partial class TerrenoAleatorio : StaticBody2D
{
    [Export] public int PuntosTerreno = 20;
    [Export] public float AnchoMapa = 1152.0f;
    [Export] public float AlturaBase = 450.0f;
    [Export] public float VariacionAltura = 80.0f;

    private Line2D _lineaVisual;
    private CollisionPolygon2D _colisionPoligono;
    private Polygon2D _rellenoVisual;

    // Paleta de colores para el relleno del terreno
    private readonly Color[] _coloresTierra = new Color[]
    {
        new Color("3d2314"), // Tierra clásica
        new Color("2d3a1e"), // Verde oscuro / Musgo
        new Color("4a2711"), // Marrón rojizo
        new Color("3a3a3a"), // Gris piedra
        new Color("5c3a21"), // Arcilla / Arena
        new Color("211a2d")  // Violeta oscuro / Noche
    };

    public override void _Ready()
    {
        GlobalPosition = Vector2.Zero;
        InicializarNodos();
        GenerarTerreno();
    }

    private void InicializarNodos()
    {
        _rellenoVisual = GetNodeOrNull<Polygon2D>("RellenoVisual");
        if (_rellenoVisual == null)
        {
            _rellenoVisual = new Polygon2D { Name = "RellenoVisual" };
            AddChild(_rellenoVisual);
        }

        _lineaVisual = GetNodeOrNull<Line2D>("LineaVisual");
        if (_lineaVisual == null)
        {
            _lineaVisual = new Line2D 
            { 
                Name = "LineaVisual", 
                Width = 8.0f, 
                DefaultColor = Colors.Green 
            };
            AddChild(_lineaVisual);
        }

        _colisionPoligono = GetNodeOrNull<CollisionPolygon2D>("ColisionPoligono");
        if (_colisionPoligono == null)
        {
            _colisionPoligono = new CollisionPolygon2D { Name = "ColisionPoligono" };
            AddChild(_colisionPoligono);
        }
    }

    public void GenerarTerreno()
    {
        // Selecciona un color aleatorio de la paleta en cada generación
        if (_rellenoVisual != null)
        {
            int indiceColor = (int)GD.RandRange(0, _coloresTierra.Length - 1);
            _rellenoVisual.Color = _coloresTierra[indiceColor];
        }

        Vector2[] puntosArriba = new Vector2[PuntosTerreno];
        float pasoX = AnchoMapa / (PuntosTerreno - 1);

        for (int i = 0; i < PuntosTerreno; i++)
        {
            float x = i * pasoX;
            float variacion = (i == 0 || i == PuntosTerreno - 1) ? 0 : (float)GD.RandRange(-VariacionAltura, VariacionAltura);
            float y = AlturaBase + variacion;

            puntosArriba[i] = new Vector2(x, y);
        }

        _lineaVisual.ClearPoints();
        foreach (Vector2 p in puntosArriba)
        {
            _lineaVisual.AddPoint(p);
        }

        Vector2[] poligonoCompleto = new Vector2[PuntosTerreno + 2];
        for (int i = 0; i < PuntosTerreno; i++)
        {
            poligonoCompleto[i] = puntosArriba[i];
        }

        poligonoCompleto[PuntosTerreno] = new Vector2(AnchoMapa, AlturaBase + 600);
        poligonoCompleto[PuntosTerreno + 1] = new Vector2(0, AlturaBase + 600);

        _colisionPoligono.Polygon = poligonoCompleto;
        _rellenoVisual.Polygon = poligonoCompleto;
    }

    public float ObtenerAlturaEnX(float x)
    {
        if (_lineaVisual == null || _lineaVisual.Points.Length == 0)
        {
            InicializarNodos();
            GenerarTerreno();
        }

        float pasoX = AnchoMapa / (PuntosTerreno - 1);
        int indice = Mathf.Clamp((int)(x / pasoX), 0, PuntosTerreno - 2);

        Vector2 p1 = _lineaVisual.GetPointPosition(indice);
        Vector2 p2 = _lineaVisual.GetPointPosition(indice + 1);

        float t = (x - p1.X) / (p2.X - p1.X);
        float yLocal = Mathf.Lerp(p1.Y, p2.Y, t);

        return GlobalPosition.Y + yLocal;
    }
}