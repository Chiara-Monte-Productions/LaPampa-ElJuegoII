using Godot;

public partial class CalculadorAlturaTerreno : Node
{
    // DIP: Dependemos de la interfaz PosicionTerreno en lugar del tipo concreto GeneradorTerreno
    [Export] public Node TerrenoNodo { get; set; }

    private PosicionTerreno _servicioTerreno;

    public override void _Ready()
    {
        ResolverServicio();
    }

    private void ResolverServicio()
    {
        if (TerrenoNodo is PosicionTerreno terreno)
        {
            _servicioTerreno = terreno;
        }
        else if (GetParent() is PosicionTerreno terrenoPadre)
        {
            _servicioTerreno = terrenoPadre;
        }
    }

    public float ObtenerAlturaEn(float x)
    {
        if (_servicioTerreno == null)
        {
            ResolverServicio();
        }

        return _servicioTerreno != null ? _servicioTerreno.ObtenerAlturaEnX(x) : 0f;
    }
}