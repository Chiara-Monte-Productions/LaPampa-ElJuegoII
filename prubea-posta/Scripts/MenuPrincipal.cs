using Godot;

public partial class MenuPrincipal : Control
{
    [ExportGroup("Componentes de Audio")]
    [Export] public GestorAudio GestorAudioLocal { get; set; }

    [ExportGroup("Botones del Menú")]
    [Export] public TextureButton BotonJugar { get; set; }
    [Export] public TextureButton BotonSalir { get; set; }

    [ExportGroup("Rutas de Escenas")]
    [Export(PropertyHint.File, "*.tscn")] 
    public string RutaEscenaJuego { get; set; } = "res://Escenas/main.tscn";

    public override void _Ready()
    {
        // 1. Inicializar servicio de audio local
        if (GestorAudioLocal == null)
        {
            GestorAudioLocal = GetNodeOrNull<GestorAudio>("GestorAudio");
        }

        GestorAudioLocal?.ReproducirMusicaMenu();

        // 2. Conectar eventos de la interfaz
        if (BotonJugar != null)
        {
            BotonJugar.Pressed += OnBotonJugarPressed;
        }

        if (BotonSalir != null)
        {
            BotonSalir.Pressed += OnBotonSalirPressed;
        }
    }

    private void OnBotonJugarPressed()
    {
        GD.Print("Cargando el nivel principal...");
        
        // Detiene la música del menú antes de cambiar de escena
        GestorAudioLocal?.DetenerMusicaMenu();

        GetTree().ChangeSceneToFile(RutaEscenaJuego);
    }

    private void OnBotonSalirPressed()
    {
        GD.Print("Cerrando el juego...");
        GetTree().Quit();
    }
}