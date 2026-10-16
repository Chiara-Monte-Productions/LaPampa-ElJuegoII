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

    [Export] public Color ColorJugadorActivo { get; set; } = Colors.Yellow;
    [Export] public Color ColorJugadorInactivo { get; set; } = Colors.Gray;

    public void ActualizarMarcador(int puntosJ1, int puntosJ2)
    {
        if (LabelPuntajeJ1 != null) LabelPuntajeJ1.Text = $"{puntosJ1}";
        if (LabelPuntajeJ2 != null) LabelPuntajeJ2.Text = $"{puntosJ2}";
    }

    public void ActualizarTurnoVisual(int turnoActual, Angulo jugadorActivo)
    {
        if (LabelTurno != null) LabelTurno.Text = $"{turnoActual}";

        if (LabelNombreJ1 != null)
            LabelNombreJ1.SelfModulate = (turnoActual == 1) ? ColorJugadorActivo : ColorJugadorInactivo;

        if (LabelNombreJ2 != null)
            LabelNombreJ2.SelfModulate = (turnoActual == 2) ? ColorJugadorActivo : ColorJugadorInactivo;

        // Cumple LSP: Depende únicamente de la interfaz Angulo recibida por parámetro
        if (jugadorActivo != null)
        {
            if (LabelAngulo != null) LabelAngulo.Text = $"{Mathf.Round(jugadorActivo.ObtenerAngulo())}°";
            if (LabelFuerza != null) LabelFuerza.Text = $"{Mathf.Round(jugadorActivo.FuerzaDisparo)}";
        }
    }

    public void ActualizarEstado(string mensaje)
    {
        if (LabelEstado != null) LabelEstado.Text = mensaje;
    }
}