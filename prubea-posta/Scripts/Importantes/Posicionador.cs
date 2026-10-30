using Godot;

public partial class Posicionador : Node, PosicionTerreno
{
    [Export] public Node NodoTerreno { get; set; }

    private PosicionTerreno ServicioTerreno => NodoTerreno as PosicionTerreno;

    public float ObtenerAlturaEnX(float x)
    {
        return ServicioTerreno != null ? ServicioTerreno.ObtenerAlturaEnX(x) : 450f;
    }

    public void AjustarObjetoAlPiso(Node2D objeto, float x)
    {
        if (objeto == null || !GodotObject.IsInstanceValid(objeto)) return;

        var espacioFisico = objeto.GetTree()?.Root?.World2D?.DirectSpaceState;
        if (espacioFisico == null)
        {
            AjustarPorCalculoDirecto(objeto, x);
            return;
        }

        Vector2 desde = new Vector2(x, 0);
        Vector2 hasta = new Vector2(x, 2000);

        var parametrosRayo = PhysicsRayQueryParameters2D.Create(desde, hasta);
        parametrosRayo.CollisionMask = 1;

        if (objeto is CollisionObject2D colisionable)
        {
            parametrosRayo.Exclude = new Godot.Collections.Array<Rid> { colisionable.GetRid() };
        }

        var resultado = espacioFisico.IntersectRay(parametrosRayo);

        if (resultado.Count > 0 && resultado.ContainsKey("position"))
        {
            Vector2 puntoImpacto = (Vector2)resultado["position"];
            objeto.GlobalPosition = new Vector2(puntoImpacto.X, puntoImpacto.Y - 20.0f);
        }
        else
        {
            AjustarPorCalculoDirecto(objeto, x);
        }
    }

    private void AjustarPorCalculoDirecto(Node2D objeto, float x)
    {
        if (ServicioTerreno != null)
        {
            float y = ServicioTerreno.ObtenerAlturaEnX(x);
            objeto.GlobalPosition = new Vector2(x, y - 20.0f);
        }
    }
}