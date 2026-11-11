using Godot;

public partial class HUDManager : Node
{
    [Export] public Label LabelNombreJ1 { get; set; }
    [Export] public Label LabelNombreJ2 { get; set; }
    [Export] public Label LabelPuntajeJ1 { get; set; }
    [Export] public Label LabelPuntajeJ2 { get; set; }
    [Export] public Label LabelTurno { get; set; }
    [Export] public Label LabelAngulo { get; set; }
    [Export] public Label LabelFuerza { get; set; }
    [Export] public Label LabelEstado { get; set; }

    // --- NUEVO: Referencia a la etiqueta del viento ---
    [Export] public Label LabelViento { get; set; }

    [Export] public Color ColorJugadorActivo { get; set; } = Colors.Yellow;
    [Export] public Color ColorJugadorInactivo { get; set; } = Colors.Gray;

    private IServicioViento _servicioViento;

    public override void _Ready()
    {
        // Buscar el servicio de viento de forma desacoplada
        if (GetTree().GetFirstNodeInGroup("ServicioViento") is IServicioViento viento)
        {
            _servicioViento = viento;
        }
    }

    public void ActualizarMarcador(int puntosJ1, int puntosJ2)
    {
        if (LabelPuntajeJ1 != null) LabelPuntajeJ1.Text = $"{puntosJ1}";
        if (LabelPuntajeJ2 != null) LabelPuntajeJ2.Text = $"{puntosJ2}";
    }

    public void ActualizarTurnoVisual(int turnoActual, Angulo jugadorActivo)
    {
        ActualizarEtiquetaTurno(turnoActual);
        ActualizarColoresNombres(turnoActual);
        ActualizarDatosJugadorActivo(jugadorActivo);
        ActualizarVientoVisual();
    }

    private void ActualizarEtiquetaTurno(int turnoActual)
    {
        if (LabelTurno != null) LabelTurno.Text = $"{turnoActual}";
    }

    private void ActualizarColoresNombres(int turnoActual)
    {
        if (LabelNombreJ1 != null)
            LabelNombreJ1.SelfModulate = (turnoActual == 1) ? ColorJugadorActivo : ColorJugadorInactivo;

        if (LabelNombreJ2 != null)
            LabelNombreJ2.SelfModulate = (turnoActual == 2) ? ColorJugadorActivo : ColorJugadorInactivo;
    }

    private void ActualizarDatosJugadorActivo(Angulo jugadorActivo)
    {
        if (jugadorActivo == null) return;

        if (LabelAngulo != null) LabelAngulo.Text = $"{Mathf.Round(jugadorActivo.ObtenerAngulo())}°";
        if (LabelFuerza != null) LabelFuerza.Text = $"{Mathf.Round(jugadorActivo.FuerzaDisparo)}";
    }

    // --- NUEVO: Método para formatear el viento ---
    private void ActualizarVientoVisual()
    {
        if (LabelViento == null) return;

        if (_servicioViento == null)
        {
            if (GetTree().GetFirstNodeInGroup("ServicioViento") is IServicioViento viento)
            {
                _servicioViento = viento;
            }
        }

        if (_servicioViento != null)
        {
            float fuerzaX = _servicioViento.FuerzaVientoActual.X;
            float magnitud = Mathf.Abs(fuerzaX);
            string flecha = fuerzaX >= 0 ? ">" : "<";

            LabelViento.Text = $"viento: {Mathf.Round(magnitud)} {flecha}";
        }
    }

    public void ActualizarEstado(string mensaje)
    {
        if (LabelEstado != null) LabelEstado.Text = mensaje;
    }
}