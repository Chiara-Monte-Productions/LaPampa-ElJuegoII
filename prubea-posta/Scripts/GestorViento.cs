using Godot;

public partial class GestorViento : Node, IServicioViento
{
    [ExportGroup("Viento Base de Ronda")]
    [Export] public float FuerzaMaximaRonda { get; set; } = 300f;
    [Export] public float FuerzaMinimaRonda { get; set; } = 50f;

    [ExportGroup("Fluctuación de Turno")]
    [Export] public float VariacionMaximaTurno { get; set; } = 30f; // Variación leve (+/- 30 unidades)

    public Vector2 FuerzaVientoActual { get; private set; } = Vector2.Zero;
    
    private Vector2 _vientoBaseRonda = Vector2.Zero;

    public override void _Ready()
    {
        AddToGroup("ServicioViento");
        GenerarVientoRonda();
    }

    public void GenerarVientoRonda()
    {
        // Viento base de la ronda: dirección fuerte (Izquierda/Derecha) y magnitud alta
        float direccion = GD.Randf() > 0.5f ? 1f : -1f;
        float magnitud = (float)GD.RandRange(FuerzaMinimaRonda, FuerzaMaximaRonda);

        _vientoBaseRonda = new Vector2(direccion * magnitud, 0f);
        FuerzaVientoActual = _vientoBaseRonda;

        GD.Print($"[Viento] NUEVA RONDA - Viento Base: {FuerzaVientoActual.X:F1}");
    }

    public void VariarVientoTurno()
    {
        // Aplica una fluctuación leve alrededor del viento base de la ronda
        float variacionX = (float)GD.RandRange(-VariacionMaximaTurno, VariacionMaximaTurno);
        
        // Mantiene el viento derivado del viento base de la ronda
        FuerzaVientoActual = new Vector2(_vientoBaseRonda.X + variacionX, 0f);

        GD.Print($"[Viento] CAMBIO DE TURNO - Viento Actualizado: {FuerzaVientoActual.X:F1} (Base: {_vientoBaseRonda.X:F1})");
    }
}