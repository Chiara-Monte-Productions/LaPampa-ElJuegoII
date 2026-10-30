using Godot;

public partial class GestorAudio : Node, IServicioAudio
{
    [ExportGroup("Música de Menú Principal")]
    [Export] public AudioStream MusicaMenuIntro { get; set; }
    [Export] public AudioStream MusicaMenuLoop { get; set; }

    [ExportGroup("Efectos de Sonido (SFX)")]
    [Export] public AudioStream SonidoDisparo { get; set; }
    [Export] public AudioStream SonidoVueloBala { get; set; }
    [Export] public AudioStream SonidoImpactoTerreno { get; set; }
    [Export] public AudioStream SonidoImpactoJugador { get; set; }

    private AudioStreamPlayer _reproductorIntro;
    private AudioStreamPlayer _reproductorLoop;
    private AudioStreamPlayer _reproductorVuelo;

    public override void _Ready()
    {
        // Registrar en el grupo local para que la Bala lo encuentre mediante IServicioAudio
        AddToGroup("ServicioAudio");

        _reproductorIntro = new AudioStreamPlayer { Bus = "Master" };
        _reproductorLoop = new AudioStreamPlayer { Bus = "Master" };
        _reproductorVuelo = new AudioStreamPlayer { Bus = "Master" };

        AddChild(_reproductorIntro);
        AddChild(_reproductorLoop);
        AddChild(_reproductorVuelo);

        _reproductorIntro.Finished += OnIntroFinalizada;
        _reproductorLoop.Finished += OnLoopFinalizado;
    }

    // --- MÚSICA DE MENÚ ---
    public void ReproducirMusicaMenu()
    {
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

    // --- EFECTOS DE SONIDO (SFX) ---

    // El disparo suena en un reproductor temporal independiente para que NO se corte
    public void ReproducirDisparo()
    {
        ReproducirEfectoPuntual(SonidoDisparo);
    }

    // El sonido de vuelo suena en un canal persistente que se interrumpe al impactar
    public void IniciarSonidoVuelo()
    {
        if (SonidoVueloBala == null) return;
        _reproductorVuelo.Stream = SonidoVueloBala;
        _reproductorVuelo.Play();
    }

    public void DetenerSonidoVuelo()
    {
        if (_reproductorVuelo.Playing)
        {
            _reproductorVuelo.Stop();
        }
    }

    // Cortan el vuelo e inician el impacto en un reproductor puntual
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

    // Método auxiliar para canales de audio independientes
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

        // Autodestrucción del reproductor cuando el clip finaliza
        reproductorEfecto.Finished += reproductorEfecto.QueueFree;
    }
}