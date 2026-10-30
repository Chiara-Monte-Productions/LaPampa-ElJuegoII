using Godot;

public partial class MenuPrincipal : Control
{
    [ExportGroup("Componentes de Audio")]
    [Export] public Node GestorAudioNodo { get; set; }

    [ExportGroup("Botones del Menú")]
    [Export] public TextureButton BotonJugar { get; set; }
    [Export] public TextureButton BotonSalir { get; set; }

    [ExportGroup("Rutas de Escenas")]
    [Export(PropertyHint.File, "*.tscn")] 
    public string RutaEscenaJuego { get; set; } = "res://Escenas/main.tscn";

    private ServicioAudio _servicioAudio;

    public override void _Ready()
    {
        if (GestorAudioNodo is ServicioAudio servicio)
        {
            _servicioAudio = servicio;
        }
        else
        {
            _servicioAudio = GetNodeOrNull<ServicioAudio>("GestorAudio");
        }

        (_servicioAudio as GestorAudio)?.ReproducirMusicaMenu();

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
        (_servicioAudio as GestorAudio)?.DetenerMusicaMenu();
        GetTree().ChangeSceneToFile(RutaEscenaJuego);
    }

    private void OnBotonSalirPressed()
    {
        GD.Print("Cerrando el juego...");
        GetTree().Quit();
    }
}