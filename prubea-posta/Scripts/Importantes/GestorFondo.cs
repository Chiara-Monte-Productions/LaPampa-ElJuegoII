using Godot;

public partial class GestorFondo : Node
{
    [Export] public Sprite2D NodoFondo { get; set; }
    [Export] public Godot.Collections.Array<Texture2D> TexturasFondo { get; set; } = new();

    public void CambiarFondoAleatorio()
    {
        if (NodoFondo == null)
        {
            GD.PrintErr("!!! GestorFondo: NodoFondo no está asignado.");
            return;
        }

        if (TexturasFondo == null || TexturasFondo.Count == 0)
        {
            GD.PrintErr("!!! GestorFondo: No hay imágenes agregadas en TexturasFondo.");
            return;
        }

        // 1. Resetear transformaciones previas del nodo
        NodoFondo.Rotation = 0f;
        NodoFondo.Scale = Vector2.One;

        // 2. Asignar imagen aleatoria
        int indiceAleatorio = GD.RandRange(0, TexturasFondo.Count - 1);
        Texture2D textura = TexturasFondo[indiceAleatorio];
        NodoFondo.Texture = textura;

        Vector2 tamanoPantalla = GetViewport().GetVisibleRect().Size;
        Vector2 tamanoTextura = textura.GetSize();

        if (tamanoTextura.X <= 0 || tamanoTextura.Y <= 0) return;

        // 3. Si la imagen es vertical (más alta que ancha), la rotamos 90 grados para apaisarla
        if (tamanoTextura.Y > tamanoTextura.X)
        {
            NodoFondo.Rotation = Mathf.DegToRad(90f);
            // Al rotar 90°, los ejes X e Y se invierten para el cálculo del escalado
            tamanoTextura = new Vector2(tamanoTextura.Y, tamanoTextura.X);
        }

        // 4. Centrar en pantalla
        NodoFondo.GlobalPosition = tamanoPantalla / 2.0f;

        // 5. Escalar exactamente para cubrir el Viewport
        NodoFondo.Scale = new Vector2(
            tamanoPantalla.X / tamanoTextura.X,
            tamanoPantalla.Y / tamanoTextura.Y
        );
    }
}