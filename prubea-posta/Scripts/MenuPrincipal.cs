using Godot;

public partial class MenuPrincipal : Control
{
    [ExportGroup("Botones del Menú")]
    [Export] public TextureButton BotonJugar { get; set; }
    [Export] public TextureButton BotonSalir { get; set; }

    [ExportGroup("Rutas de Escenas")]
    [Export(PropertyHint.File, "*.tscn")] 
    public string RutaEscenaJuego { get; set; } = "res://Escenas/main.tscn";

    public override void _Ready()
    {
        // Conectar eventos de clic
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
        GetTree().ChangeSceneToFile(RutaEscenaJuego);
    }

    private void OnBotonSalirPressed()
    {
        GD.Print("Cerrando el juego...");
        GetTree().Quit();
    }
}