using Godot;

public partial class GameManager : Node
{
    [Export] public Player Jugador1 { get; set; }
    [Export] public Player Jugador2 { get; set; }
    [Export] public Label TextoInfo { get; set; }
    [Export] public TerrenoAleatorio Terreno { get; set; }

    // Ruta para regresar al Menú Principal con la tecla ESC
    [Export] public string RutaMenuPrincipal = "res://MenuPrincipal.tscn";

    private static int _puntosJ1 = 0;
    private static int _puntosJ2 = 0;

    private int _turnoActual = 1;
    private bool _balaEnVuelo = false;
    private bool _rondaFinalizada = false;

    public override async void _Ready()
    {
        // Esperamos a que el motor físico procese la colisión del terreno antes de posicionar
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
        // Detecta la tecla ESC (mapeada en Godot como "ui_cancel")
        if (@event.IsActionPressed("ui_cancel"))
        {
            VolverAlMenu();
        }
    }

    private void VolverAlMenu()
    {
        // Reiniciamos los puntos globales al volver al menú
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

        // Lanzamos el rayo desde Y = 0 para no colisionar con elementos del HUD
        Vector2 desde = new Vector2(x, 0);
        Vector2 hasta = new Vector2(x, 1200);

        var parametrosRayo = PhysicsRayQueryParameters2D.Create(desde, hasta);
        parametrosRayo.CollisionMask = 1; // Revisa únicamente la Capa 1 (Terreno)
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
            ActualizarTextoHUD();
        }
    }

    private void IniciarTurno()
    {
        if (_rondaFinalizada) return;

        _balaEnVuelo = false;

        // Si el jugador del turno actual ya no existe, reevaluamos el estado de la partida
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

        ActualizarTextoHUD();
    }

    private void ActualizarTextoHUD()
    {
        if (TextoInfo == null) return;

        Player jugadorActivo = (_turnoActual == 1) ? Jugador1 : Jugador2;

        if (GodotObject.IsInstanceValid(jugadorActivo))
        {
            float angulo = jugadorActivo.ObtenerAngulo();
            float fuerza = Mathf.Round(jugadorActivo.FuerzaDisparo);

            TextoInfo.Text = $"[J1: {_puntosJ1} pts | J2: {_puntosJ2} pts]  -  Turno: Jugador {_turnoActual}  |  Ángulo: {angulo}°  |  Fuerza: {fuerza}";
        }
    }

    private void OnBalaLanzada(Bala bala)
    {
        _balaEnVuelo = true;

        if (GodotObject.IsInstanceValid(Jugador1)) Jugador1.EsMiTurno = false;
        if (GodotObject.IsInstanceValid(Jugador2)) Jugador2.EsMiTurno = false;

        if (TextoInfo != null)
        {
            TextoInfo.Text = $"[J1: {_puntosJ1} pts | J2: {_puntosJ2} pts]  -  Bala en vuelo...";
        }

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
                if (TextoInfo != null) TextoInfo.Text = "¡EMPATE! Reiniciando ronda...";
            }
            else if (!j1Vivo)
            {
                _puntosJ2++;
                if (TextoInfo != null) TextoInfo.Text = $"¡JUGADOR 2 GANA LA RONDA! [{_puntosJ1} - {_puntosJ2}]";
            }
            else if (!j2Vivo)
            {
                _puntosJ1++;
                if (TextoInfo != null) TextoInfo.Text = $"¡JUGADOR 1 GANA LA RONDA! [{_puntosJ1} - {_puntosJ2}]";
            }

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