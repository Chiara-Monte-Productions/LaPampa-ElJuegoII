using Godot;

public partial class GestorAudio : Node, ServicioAudio
{
    [ExportGroup("Música de Menú Principal")]
    [Export] public AudioStream MusicaMenuIntro { get; set; }
    [Export] public AudioStream MusicaMenuLoop { get; set; }

    [ExportGroup("Música de Juego")]
    [Export] public AudioStream MusicaJuego { get; set; }

    [ExportGroup("Efectos de Sonido (SFX)")]
    [Export] public AudioStream SonidoDisparo { get; set; }
    [Export] public AudioStream SonidoVueloBala { get; set; }
    [Export] public AudioStream SonidoImpactoTerreno { get; set; }
    [Export] public AudioStream SonidoImpactoJugador { get; set; }

    private AudioStreamPlayer _reproductorIntro;
    private AudioStreamPlayer _reproductorLoop;
    private AudioStreamPlayer _reproductorVuelo;
    private AudioStreamPlayer _reproductorMusicaJuego;

    public override void _Ready()
    {
        AddToGroup("ServicioAudio");

        _reproductorIntro = new AudioStreamPlayer { Bus = "Master" };
        _reproductorLoop = new AudioStreamPlayer { Bus = "Master" };
        _reproductorVuelo = new AudioStreamPlayer { Bus = "Master" };
        _reproductorMusicaJuego = new AudioStreamPlayer { Bus = "Master" };

        AddChild(_reproductorIntro);
        AddChild(_reproductorLoop);
        AddChild(_reproductorVuelo);
        AddChild(_reproductorMusicaJuego);

        _reproductorIntro.Finished += OnIntroFinalizada;
        _reproductorLoop.Finished += OnLoopFinalizado;

        // Auto-detección: si la escena asignó pistas de menú, reproduce el menú. 
        // De lo contrario, si asignó música de juego, reproduce la música de juego.
        if (MusicaMenuIntro != null || MusicaMenuLoop != null)
        {
            ReproducirMusicaMenu();
        }
        else if (MusicaJuego != null)
        {
            ReproducirMusicaJuego();
        }
    }

    // --- MÚSICA DE MENÚ ---

    public void ReproducirMusicaMenu()
    {
        DetenerMusicaJuego();
        DetenerMusicaMenu();

        if (MusicaMenuIntro != null)
        {
            _reproductorIntro.Stream = MusicaMenuIntro;
            _reproductorIntro.Play();
        }
        else if (MusicaMenuLoop != null)
        {
            IniciarLoop();
        }
    }

    private void OnIntroFinalizada() => IniciarLoop();

    private void IniciarLoop()
    {
        if (MusicaMenuLoop == null) return;
        _reproductorLoop.Stream = MusicaMenuLoop;
        _reproductorLoop.Play();
    }

    private void OnLoopFinalizado()
    {
        if (MusicaMenuLoop != null) _reproductorLoop.Play();
    }

    public void DetenerMusicaMenu()
    {
        if (_reproductorIntro.Playing) _reproductorIntro.Stop();
        if (_reproductorLoop.Playing) _reproductorLoop.Stop();
    }

    // --- MÚSICA DE JUEGO ---

    public void ReproducirMusicaJuego()
    {
        DetenerMusicaMenu();

        if (MusicaJuego != null)
        {
            _reproductorMusicaJuego.Stream = MusicaJuego;
            _reproductorMusicaJuego.Play();
        }
    }

    public void DetenerMusicaJuego()
    {
        if (_reproductorMusicaJuego.Playing) _reproductorMusicaJuego.Stop();
    }

    // --- EFECTOS DE SONIDO (SFX) ---

    public void ReproducirDisparo() => ReproducirEfectoPuntual(SonidoDisparo);

    public void IniciarSonidoVuelo()
    {
        if (SonidoVueloBala == null) return;
        _reproductorVuelo.Stream = SonidoVueloBala;
        _reproductorVuelo.Play();
    }

    public void DetenerSonidoVuelo()
    {
        if (_reproductorVuelo.Playing) _reproductorVuelo.Stop();
    }

    public void ReproducirImpactoTerreno()
    {
        DetenerSonidoVuelo();
        ReproducirEfectoPuntual(SonidoImpactoTerreno);
    }

    public void ReproducirImpactoJugador()
    {
        DetenerSonidoVuelo();
        ReproducirEfectoPuntual(SonidoImpactoJugador);
    }

    private void ReproducirEfectoPuntual(AudioStream stream)
    {
        if (stream == null) return;

        AudioStreamPlayer reproductorEfecto = new AudioStreamPlayer
        {
            Stream = stream,
            Bus = "Master"
        };

        AddChild(reproductorEfecto);
        reproductorEfecto.Play();
        reproductorEfecto.Finished += reproductorEfecto.QueueFree;
    }
}