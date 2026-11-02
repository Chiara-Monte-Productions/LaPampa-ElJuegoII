using Godot;

public partial class GestorFondo : Node
{
    [Export] public Sprite2D NodoFondo { get; set; }
    [Export] public Godot.Collections.Array<Texture2D> TexturasFondo { get; set; } = new();

    public void CambiarFondoAleatorio()
    {
        if (!ValidarConfiguracion()) return;

        ResetearTransformaciones();
        Texture2D textura = ObtenerTexturaAleatoria();
        NodoFondo.Texture = textura;

        Vector2 tamanoPantalla = GetViewport().GetVisibleRect().Size;
        Vector2 tamanoTextura = AjustarOrientacionTextura(textura);

        AplicarPosicionYEscala(tamanoPantalla, tamanoTextura);
    }

    private bool ValidarConfiguracion()
    {
        if (NodoFondo == null)
        {
            GD.PrintErr("!!! GestorFondo: NodoFondo no está asignado.");
            return false;
        }

        if (TexturasFondo == null || TexturasFondo.Count == 0)
        {
            GD.PrintErr("!!! GestorFondo: No hay imágenes agregadas en TexturasFondo.");
            return false;
        }

        return true;
    }

    private void ResetearTransformaciones()
    {
        NodoFondo.Rotation = 0f;
        NodoFondo.Scale = Vector2.One;
    }

    private Texture2D ObtenerTexturaAleatoria()
    {
        int indiceAleatorio = GD.RandRange(0, TexturasFondo.Count - 1);
        return TexturasFondo[indiceAleatorio];
    }

    private Vector2 AjustarOrientacionTextura(Texture2D textura)
    {
        Vector2 tamanoTextura = textura.GetSize();

        if (tamanoTextura.Y > tamanoTextura.X)
        {
            NodoFondo.Rotation = Mathf.DegToRad(90f);
            return new Vector2(tamanoTextura.Y, tamanoTextura.X);
        }

        return tamanoTextura;
    }

    private void AplicarPosicionYEscala(Vector2 tamanoPantalla, Vector2 tamanoTextura)
    {
        if (tamanoTextura.X <= 0 || tamanoTextura.Y <= 0) return;

        NodoFondo.GlobalPosition = tamanoPantalla / 2.0f;
        NodoFondo.Scale = new Vector2(
            tamanoPantalla.X / tamanoTextura.X,
            tamanoPantalla.Y / tamanoTextura.Y
        );
    }
}