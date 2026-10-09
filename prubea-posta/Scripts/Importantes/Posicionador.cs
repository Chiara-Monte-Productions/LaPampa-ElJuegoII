using Godot;

public partial class Posicionador : Node, PosicionTerreno
{
    [Export] public TerrenoAleatorio Terreno { get; set; }

    public void AjustarObjetoAlPiso(Node2D objeto, float x)
    {
        if (objeto == null) return;

        var espacioFisico = objeto.GetTree().Root.World2D.DirectSpaceState;
        
        // El raycast inicia desde Y=0 hasta Y=2000 para cubrir toda la pantalla
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
            // Respaldo directo si el motor de física aún no procesó la colisión
            float y = Terreno.ObtenerAlturaEnX(x);
            objeto.GlobalPosition = new Vector2(x, y - 20.0f);
        }
    }
}