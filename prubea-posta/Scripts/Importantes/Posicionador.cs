using Godot;

public partial class Posicionador : Node, PosicionTerreno
{
    [Export] public Node NodoTerreno { get; set; }
    [Export] public float OffsetAltura { get; set; } = 15.0f;

    private PosicionTerreno ServicioTerreno => NodoTerreno as PosicionTerreno;

    public float ObtenerAlturaEnX(float x)
    {
        return ServicioTerreno != null ? ServicioTerreno.ObtenerAlturaEnX(x) : 450f;
    }

    public void AjustarObjetoAlPiso(Node2D objeto, float x)
    {
        if (objeto == null || !GodotObject.IsInstanceValid(objeto)) return;

        float alturaY = ObtenerAlturaEnX(x);
        objeto.GlobalPosition = new Vector2(x, alturaY - OffsetAltura);
    }
}