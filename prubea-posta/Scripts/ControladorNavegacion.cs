using Godot;

public partial class ControladorNavegacion : Node
{
    [Export] public string RutaMenuPrincipal = "res://Escenas/MenuPrincipal.tscn";

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GetTree().ChangeSceneToFile(RutaMenuPrincipal);
        }
    }
}