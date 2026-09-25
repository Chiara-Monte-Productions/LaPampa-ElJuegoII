using Godot;

public partial class GameManager : Node
{
    [ExportGroup("Jugadores y Terreno")]
    [Export] public Player Jugador1 { get; set; }
    [Export] public Player Jugador2 { get; set; }
    [Export] public TerrenoAleatorio Terreno { get; set; }

    [ExportGroup("Elementos del HUD")]
    [Export] public Label LabelNombreJ1 { get; set; } // Nombre/Texto del J1
    [Export] public Label LabelNombreJ2 { get; set; } // Nombre/Texto del J2
    [Export] public Label LabelPuntajeJ1 { get; set; }
    [Export] public Label LabelPuntajeJ2 { get; set; }
    [Export] public Label LabelTurno { get; set; }
    [Export] public Label LabelAngulo { get; set; }
    [Export] public Label LabelFuerza { get; set; }
    [Export] public Label LabelEstado { get; set; }

    [ExportGroup("Colores de Turno")]
    [Export] public Color ColorJugadorActivo { get; set; } = Colors.Yellow; // Color al estar activo
    [Export] public Color ColorJugadorInactivo { get; set; } = Colors.Gray;  // Color al estar inactivo

    [ExportGroup("Navegación")]
    [Export] public string RutaMenuPrincipal = "res://MenuPrincipal.tscn";

    private static int _puntosJ1 = 0;
    private static int _puntosJ2 = 0;

    private int _turnoActual = 1;
    private bool _balaEnVuelo = false;
    private bool _rondaFinalizada = false;

    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        PosicionarJugadoresEnTerreno();

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

    private void PosicionarJugadoresEnTerreno()
    {
        if (Terreno == null) return;

        float xJ1 = (float)GD.RandRange(100.0, 400.0);
        float xJ2 = (float)GD.RandRange(750.0, 1050.0);

        if (GodotObject.IsInstanceValid(Jugador1)) AjustarJugadorAlPiso(Jugador1, xJ1);
        if (GodotObject.IsInstanceValid(Jugador2)) AjustarJugadorAlPiso(Jugador2, xJ2);
    }

    private void AjustarJugadorAlPiso(Player jugador, float x)
    {
        var espacioFisico = GetTree().Root.World2D.DirectSpaceState;

        Vector2 desde = new Vector2(x, 0);
        Vector2 hasta = new Vector2(x, 1200);

        var parametrosRayo = PhysicsRayQueryParameters2D.Create(desde, hasta);
        parametrosRayo.CollisionMask = 1;
        parametrosRayo.Exclude = new Godot.Collections.Array<Rid> { jugador.GetRid() };

        var resultado = espacioFisico.IntersectRay(parametrosRayo);

        if (resultado.Count > 0)
        {
            Vector2 puntoImpacto = (Vector2)resultado["position"];
            jugador.GlobalPosition = new Vector2(puntoImpacto.X, puntoImpacto.Y - 20.0f);
        }
        else
        {
            float y = Terreno.ObtenerAlturaEnX(x);
            jugador.GlobalPosition = new Vector2(x, y - 20.0f);
        }
    }

    public override void _Process(double delta)
    {
        if (!_balaEnVuelo && !_rondaFinalizada)
        {
            ActualizarHUD();
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

        if (_turnoActual == 1)
        {
            if (GodotObject.IsInstanceValid(Jugador1)) Jugador1.EsMiTurno = true;
            if (GodotObject.IsInstanceValid(Jugador2)) Jugador2.EsMiTurno = false;
        }
        else
        {
            if (GodotObject.IsInstanceValid(Jugador1)) Jugador1.EsMiTurno = false;
            if (GodotObject.IsInstanceValid(Jugador2)) Jugador2.EsMiTurno = true;
        }

        if (LabelEstado != null) LabelEstado.Text = "Esperando disparo...";
        ActualizarHUD();
    }

    private void ActualizarHUD()
    {
        if (LabelPuntajeJ1 != null) LabelPuntajeJ1.Text = $"{_puntosJ1}";
        if (LabelPuntajeJ2 != null) LabelPuntajeJ2.Text = $"{_puntosJ2}";
        if (LabelTurno != null) LabelTurno.Text = $"{_turnoActual}";

        // Cambiar color de las etiquetas de los jugadores según el turno
        if (LabelNombreJ1 != null)
        {
            LabelNombreJ1.SelfModulate = (_turnoActual == 1) ? ColorJugadorActivo : ColorJugadorInactivo;
        }

        if (LabelNombreJ2 != null)
        {
            LabelNombreJ2.SelfModulate = (_turnoActual == 2) ? ColorJugadorActivo : ColorJugadorInactivo;
        }

        Player jugadorActivo = (_turnoActual == 1) ? Jugador1 : Jugador2;

        if (GodotObject.IsInstanceValid(jugadorActivo))
        {
            if (LabelAngulo != null) LabelAngulo.Text = $"{jugadorActivo.ObtenerAngulo()}";
            if (LabelFuerza != null) LabelFuerza.Text = $"{Mathf.Round(jugadorActivo.FuerzaDisparo)}";
        }
    }

    private void OnBalaLanzada(Bala bala)
    {
        _balaEnVuelo = true;

        if (GodotObject.IsInstanceValid(Jugador1)) Jugador1.EsMiTurno = false;
        if (GodotObject.IsInstanceValid(Jugador2)) Jugador2.EsMiTurno = false;

        if (LabelEstado != null) LabelEstado.Text = "Bala en vuelo...";

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
                if (LabelEstado != null) LabelEstado.Text = "¡EMPATE!";
            }
            else if (!j1Vivo)
            {
                _puntosJ2++;
                if (LabelEstado != null) LabelEstado.Text = "¡JUGADOR 2 GANA!";
            }
            else if (!j2Vivo)
            {
                _puntosJ1++;
                if (LabelEstado != null) LabelEstado.Text = "¡JUGADOR 1 GANA!";
            }

            ActualizarHUD();
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