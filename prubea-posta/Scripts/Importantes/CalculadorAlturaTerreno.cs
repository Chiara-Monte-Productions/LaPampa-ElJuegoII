using Godot;
using System.Collections.Generic;

public partial class CalculadorAlturaTerreno : Node, PosicionTerreno
{
    [Export] public GeneradorTerreno Terreno { get; set; }

    public float ObtenerAlturaEnX(float x)
    {
        if (Terreno == null || Terreno.PuntosTerreno == null || Terreno.PuntosTerreno.Count < 2) 
            return Terreno != null ? Terreno.AlturaBase : 450f;

        List<Vector2> puntos = Terreno.PuntosTerreno;

        for (int i = 0; i < puntos.Count - 1; i++)
        {
            if (x >= puntos[i].X && x <= puntos[i + 1].X)
            {
                float t = (x - puntos[i].X) / (puntos[i + 1].X - puntos[i].X);
                return Mathf.Lerp(puntos[i].Y, puntos[i + 1].Y, t);
            }
        }

        return Terreno.AlturaBase;
    }

    public void AjustarObjetoAlPiso(Node2D objeto, float x)
    {
        if (objeto == null) return;

        float alturaY = ObtenerAlturaEnX(x);
        objeto.GlobalPosition = new Vector2(x, alturaY);
    }
}