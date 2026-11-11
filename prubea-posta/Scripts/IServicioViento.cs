using Godot;

public interface IServicioViento
{
    Vector2 FuerzaVientoActual { get; }

    /// <summary>
    /// Establece un viento base completamente nuevo y aleatorio (se llama al iniciar la partida/ronda).
    /// </summary>
    void GenerarVientoRonda();

    /// <summary>
    /// Aplica una variación sutil/leve sobre el viento base actual (se llama en cada cambio de turno).
    /// </summary>
    void VariarVientoTurno();
}