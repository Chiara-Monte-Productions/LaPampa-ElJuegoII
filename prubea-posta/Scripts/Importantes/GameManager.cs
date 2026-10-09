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

    [ExportGroup("Navegación")]
    [Export] public string RutaMenuPrincipal = "res://Escenas/MenuPrincipal.tscn";

    private static int _puntosJ1 = 0;
    private static int _puntosJ2 = 0;

    private int _turnoActual = 1;
    private bool _balaEnVuelo = false;
    private bool _rondaFinalizada = false;

    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Cambiamos el fondo al iniciar la ronda
        if (ServicioFondo != null)
        {
            ServicioFondo.CambiarFondoAleatorio();
        }

        PosicionarJugadores();

        if (GodotObject.IsInstanceValid(Jugador1))
        {
            Jugador1.DisparoRealizado += OnBalaLanzada;
            Jugador1.JugadorMuerto += OnJugadorEliminado;
        }

        if (GodotObject.IsInstanceValid(Jugador2))
        {
            Jugador2.DisparoRealizado += OnBalaLanzada;
            Jugador2.JugadorMuerto += OnJugadorEliminado;
        }

        IniciarTurno();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            VolverAlMenu();
        }
    }

    private void VolverAlMenu()
    {
        _puntosJ1 = 0;
        _puntosJ2 = 0;
        GetTree().ChangeSceneToFile(RutaMenuPrincipal);
    }

    private void PosicionarJugadores()
    {
        if (PosicionadorTerreno == null) return;

        float xJ1 = (float)GD.RandRange(100.0, 400.0);
        float xJ2 = (float)GD.RandRange(750.0, 1050.0);

        if (GodotObject.IsInstanceValid(Jugador1)) PosicionadorTerreno.AjustarObjetoAlPiso(Jugador1, xJ1);
        if (GodotObject.IsInstanceValid(Jugador2)) PosicionadorTerreno.AjustarObjetoAlPiso(Jugador2, xJ2);
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

        if (HUD != null) HUD.ActualizarEstado("Bala en vuelo...");

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
                if (HUD != null) HUD.ActualizarEstado("¡EMPATE!");
            }
            else if (!j1Vivo)
            {
                _puntosJ2++;
                if (HUD != null) HUD.ActualizarEstado("¡JUGADOR 2 GANA!");
            }
            else if (!j2Vivo)
            {
                _puntosJ1++;
                if (HUD != null) HUD.ActualizarEstado("¡JUGADOR 1 GANA!");
            }

            if (HUD != null) HUD.ActualizarMarcador(_puntosJ1, _puntosJ2);
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