using Godot;

public partial class Posicionador : Node, PosicionTerreno
{
    // Godot no puede exportar interfaces, cambiamos a la clase concreta CalculadorAlturaTerreno
    [Export] public CalculadorAlturaTerreno Terreno { get; set; }

    public float ObtenerAlturaEnX(float x)
    {
        return Terreno != null ? Terreno.ObtenerAlturaEnX(x) : 450f;
    }

    public void AjustarObjetoAlPiso(Node2D objeto, float x)
    {
        if (objeto == null) return;

        var espacioFisico = objeto.GetTree().Root.World2D.DirectSpaceState;
        
        Vector2 desde = new Vector2(x, 0);
        Vector2 hasta = new Vector2(x, 2000);

        var parametrosRayo = PhysicsRayQueryParameters2D.Create(desde, hasta);
        parametrosRayo.CollisionMask = 1;

        if (objeto is CollisionObject2D colisionable)
        {
            parametrosRayo.Exclude = new Godot.Collections.Array<Rid> { colisionable.GetRid() };
        }

        var resultado = espacioFisico.IntersectRay(parametrosRayo);

        if (resultado.Count > 0)
        {
            Vector2 puntoImpacto = (Vector2)resultado["position"];
            objeto.GlobalPosition = new Vector2(puntoImpacto.X, puntoImpacto.Y - 20.0f);
        }
        else if (Terreno != null)
        {
            float y = Terreno.ObtenerAlturaEnX(x);
            objeto.GlobalPosition = new Vector2(x, y - 20.0f);
        }
    }
}