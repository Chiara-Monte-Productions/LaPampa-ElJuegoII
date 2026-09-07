using Godot;

public partial class MenuPrincipal : Control
{
    [Export] public string RutaEscenaJuego = "res://main.tscn"; // Ruta a tu escena de combate

    public void OnBotonJugarPressed()
    {
        GetTree().ChangeSceneToFile(RutaEscenaJuego);
    }

    public void OnBotonSalirPressed()
    {
        GetTree().Quit();
    }
}