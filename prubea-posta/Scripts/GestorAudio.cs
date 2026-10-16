using Godot;

public partial class GestorAudio : Node
{
    [ExportGroup("Música de Menú")]
    [Export] public AudioStream MusicaMenu { get; set; }

    [ExportGroup("Efectos de Sonido (SFX)")]
    [Export] public AudioStream SonidoDisparo { get; set; }
    [Export] public AudioStream SonidoVueloBala { get; set; }
    [Export] public AudioStream SonidoImpactoTerreno { get; set; }
    [Export] public AudioStream SonidoImpactoJugador { get; set; }

    private AudioStreamPlayer _reproductorMusica;
    private AudioStreamPlayer _reproductorVuelo;

    public override void _Ready()
    {
        // Canal dedicado a la música de fondo en loop
        _reproductorMusica = new AudioStreamPlayer { Bus = "Master" };
        AddChild(_reproductorMusica);

        // Canal dedicado a sonidos continuos/loops de la bala en vuelo
        _reproductorVuelo = new AudioStreamPlayer { Bus = "Master" };
        AddChild(_reproductorVuelo);
    }

    // --- MÚSICA DE MENÚ (MP3 EN LOOP) ---
    public void ReproducirMusicaMenu()
    {
        if (MusicaMenu == null) return;

        _reproductorMusica.Stream = MusicaMenu;
        _reproductorMusica.Play();
    }

    public void DetenerMusicaMenu()
    {
        _reproductorMusica.Stop();
    }

    // --- EFECTOS DE SONIDO DE LA BALA Y JUEGO ---
    public void ReproducirDisparo()
    {
        ReproducirEfectoPuntual(SonidoDisparo);
    }

    public void IniciarSonidoVuelo()
    {
        if (SonidoVueloBala == null) return;
        _reproductorVuelo.Stream = SonidoVueloBala;
        _reproductorVuelo.Play();
    }

    public void DetenerSonidoVuelo()
    {
        _reproductorVuelo.Stop();
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

    // Método privado auxiliar para instanciar SFX independientes (evita corte entre sonidos)
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

        // Autodestruir el nodo AudioStreamPlayer al finalizar el sonido
        reproductorEfecto.Finished += reproductorEfecto.QueueFree;
    }
}