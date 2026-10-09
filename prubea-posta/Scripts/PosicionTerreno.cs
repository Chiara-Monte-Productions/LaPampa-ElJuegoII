using Godot;

public interface PosicionTerreno
{
    float ObtenerAlturaEnX(float x);
    void AjustarObjetoAlPiso(Node2D objeto, float x);
}