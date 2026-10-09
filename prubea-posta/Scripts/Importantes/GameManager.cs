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

        // Cambiamos el fondo al iniciar la ronda a través del servicio dedicado
        ServicioFondo?.CambiarFondoAleatorio();

        PosicionarJugadores();

        SuscribirEventosJugador(Jugador1);
        SuscribirEventosJugador(Jugador2);

        IniciarTurno();
    }

    private void SuscribirEventosJugador(Player jugador)
    {
        if (GodotObject.IsInstanceValid(jugador))
        {
            jugador.DisparoRealizado += OnBalaLanzada;
            jugador.JugadorMuerto += OnJugadorEliminado;
        }
    }

    private void PosicionarJugadores()
    {
        if (PosicionadorTerreno == null) return;

        // El GameManager solicita el posicionamiento sin hardcodear lógica de terreno
        if (GodotObject.IsInstanceValid(Jugador1)) PosicionadorTerreno.AjustarObjetoAlPiso(Jugador1, (float)GD.RandRange(100.0, 400.0));
        if (GodotObject.IsInstanceValid(Jugador2)) PosicionadorTerreno.AjustarObjetoAlPiso(Jugador2, (float)GD.RandRange(750.0, 1050.0));
    }

    public override void _Process(double delta)
    {
        if (!_balaEnVuelo && !_rondaFinalizada && HUD != null)
        {
            Player jugadorActivo = (_turnoActual == 1) ? Jugador1 : Jugador2;
            HUD.ActualizarTurnoVisual(_turnoActual, jugadorActivo);
        }
    }

    private void IniciarTurno()
    {
        if (_rondaFinalizada) return;

        _balaEnVuelo = false;

        Player jugadorActivo = (_turnoActual == 1) ? Jugador1 : Jugador2;
        if (!GodotObject.IsInstanceValid(jugadorActivo))
        {
            VerificarEstadoPartida();
            return;
        }

        if (GodotObject.IsInstanceValid(Jugador1)) Jugador1.EsMiTurno = (_turnoActual == 1);
        if (GodotObject.IsInstanceValid(Jugador2)) Jugador2.EsMiTurno = (_turnoActual == 2);

        if (HUD != null)
        {
            HUD.ActualizarEstado("Esperando disparo...");
            HUD.ActualizarMarcador(_puntosJ1, _puntosJ2);
            HUD.ActualizarTurnoVisual(_turnoActual, jugadorActivo);
        }
    }

    private void OnBalaLanzada(Bala bala)
    {
        _balaEnVuelo = true;

        if (GodotObject.IsInstanceValid(Jugador1)) Jugador1.EsMiTurno = false;
        if (GodotObject.IsInstanceValid(Jugador2)) Jugador2.EsMiTurno = false;

        HUD?.ActualizarEstado("Bala en vuelo...");

        bala.TreeExited += OnBalaDestruida;
    }

    private void OnBalaDestruida()
    {
        VerificarEstadoPartida();
    }

    private void OnJugadorEliminado(Player jugador)
    {
        VerificarEstadoPartida();
    }

    private void VerificarEstadoPartida()
    {
        if (_rondaFinalizada) return;

        bool j1Vivo = GodotObject.IsInstanceValid(Jugador1);
        bool j2Vivo = GodotObject.IsInstanceValid(Jugador2);

        if (!j1Vivo || !j2Vivo)
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
            else if (!j2Vivo)
            {
                _puntosJ1++;
                HUD?.ActualizarEstado("¡JUGADOR 1 GANA!");
            }

            HUD?.ActualizarMarcador(_puntosJ1, _puntosJ2);
            GetTree().CreateTimer(2.0f).Timeout += ReiniciarRonda;
            return;
        }

        _turnoActual = (_turnoActual == 1) ? 2 : 1;
        IniciarTurno();
    }

    private void ReiniciarRonda()
    {
        GetTree().ReloadCurrentScene();
    }
}