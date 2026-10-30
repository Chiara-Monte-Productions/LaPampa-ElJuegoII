using Godot;

public partial class GameManager : Node
{
    [ExportGroup("Jugadores")]
    [Export] public Player Jugador1 { get; set; }
    [Export] public Player Jugador2 { get; set; }

    [ExportGroup("Servicios")]
    [Export] public HUDManager HUD { get; set; }
    [Export] public Posicionador PosicionadorTerreno { get; set; }
    [Export] public GestorFondo ServicioFondo { get; set; }

    private static int _puntosJ1 = 0;
    private static int _puntosJ2 = 0;

    private int _turnoActual = 1;
    private bool _balaEnVuelo = false;
    private bool _rondaFinalizada = false;

    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        ServicioFondo?.CambiarFondoAleatorio();
        PosicionarJugadores();

        SuscribirEventosJugador(Jugador1);
        SuscribirEventosJugador(Jugador2);

        IniciarTurno();
    }

    public override void _Process(double delta)
    {
        if (_balaEnVuelo || _rondaFinalizada || HUD == null) return;

        Player jugadorActivo = ObtenerJugadorPorTurno(_turnoActual);
        HUD.ActualizarTurnoVisual(_turnoActual, jugadorActivo);
    }

    private void PosicionarJugadores()
    {
        if (PosicionadorTerreno == null) return;

        if (EsJugadorValido(Jugador1)) PosicionadorTerreno.AjustarObjetoAlPiso(Jugador1, (float)GD.RandRange(100.0, 400.0));
        if (EsJugadorValido(Jugador2)) PosicionadorTerreno.AjustarObjetoAlPiso(Jugador2, (float)GD.RandRange(750.0, 1050.0));
    }

    private void SuscribirEventosJugador(Player jugador)
    {
        if (EsJugadorValido(jugador))
        {
            jugador.DisparoRealizado += OnBalaLanzada;
            jugador.JugadorMuerto += OnJugadorEliminado;
        }
    }

    private void IniciarTurno()
    {
        if (_rondaFinalizada) return;

        _balaEnVuelo = false;
        Player jugadorActivo = ObtenerJugadorPorTurno(_turnoActual);

        if (!EsJugadorValido(jugadorActivo))
        {
            VerificarEstadoPartida();
            return;
        }

        ActualizarEstadoTurnoJugadores();
        ActualizarHUDInicioTurno(jugadorActivo);
    }

    private void ActualizarEstadoTurnoJugadores()
    {
        if (EsJugadorValido(Jugador1)) Jugador1.EsMiTurno = (_turnoActual == 1);
        if (EsJugadorValido(Jugador2)) Jugador2.EsMiTurno = (_turnoActual == 2);
    }

    private void ActualizarHUDInicioTurno(Player jugadorActivo)
    {
        if (HUD == null) return;

        HUD.ActualizarEstado("Esperando disparo...");
        HUD.ActualizarMarcador(_puntosJ1, _puntosJ2);
        HUD.ActualizarTurnoVisual(_turnoActual, jugadorActivo);
    }

    private void OnBalaLanzada(Bala bala)
    {
        _balaEnVuelo = true;
        DesactivarTurnoAmbosJugadores();

        HUD?.ActualizarEstado("Bala en vuelo...");
        bala.TreeExited += OnBalaDestruida;
    }

    private void DesactivarTurnoAmbosJugadores()
    {
        if (EsJugadorValido(Jugador1)) Jugador1.EsMiTurno = false;
        if (EsJugadorValido(Jugador2)) Jugador2.EsMiTurno = false;
    }

    private void OnBalaDestruida() => VerificarEstadoPartida();

    private void OnJugadorEliminado(Player jugador) => VerificarEstadoPartida();

    private void VerificarEstadoPartida()
    {
        if (_rondaFinalizada) return;

        bool j1Vivo = EsJugadorValido(Jugador1);
        bool j2Vivo = EsJugadorValido(Jugador2);

        if (!j1Vivo || !j2Vivo)
        {
            ProcesarFinDeRonda(j1Vivo, j2Vivo);
            return;
        }

        CambiarSiguienteTurno();
    }

    private void ProcesarFinDeRonda(bool j1Vivo, bool j2Vivo)
    {
        _rondaFinalizada = true;

        if (!j1Vivo && !j2Vivo)
        {
            HUD?.ActualizarEstado("¡EMPATE!");
        }
        else if (!j1Vivo)
        {
            _puntosJ2++;
            HUD?.ActualizarEstado("¡JUGADOR 2 GANA!");
        }
        else
        {
            _puntosJ1++;
            HUD?.ActualizarEstado("¡JUGADOR 1 GANA!");
        }

        HUD?.ActualizarMarcador(_puntosJ1, _puntosJ2);
        GetTree().CreateTimer(2.0f).Timeout += ReiniciarRonda;
    }

    private void CambiarSiguienteTurno()
    {
        _turnoActual = (_turnoActual == 1) ? 2 : 1;
        IniciarTurno();
    }

    private bool EsJugadorValido(Player jugador)
    {
        return GodotObject.IsInstanceValid(jugador) && !jugador.IsQueuedForDeletion();
    }

    private Player ObtenerJugadorPorTurno(int turno)
    {
        return (turno == 1) ? Jugador1 : Jugador2;
    }

    private void ReiniciarRonda() => GetTree().ReloadCurrentScene();
}