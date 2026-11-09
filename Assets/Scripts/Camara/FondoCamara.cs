using UnityEngine;

// Hace que el fondo ocupe toda la pantalla, sea del tamaño que sea
public class FondoCamara : MonoBehaviour
{
    void Update()
    {
        Camera cam = Camera.main;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Lo que se ve en pantalla
        float alto = cam.orthographicSize * 2f;

        float ancho = alto * cam.aspect;

        // Tamaño de la imagen
        float anchoImagen = sr.sprite.bounds.size.x;

        float altoImagen = sr.sprite.bounds.size.y;

        // Cogemos la escala más grande para que no quede ningún hueco
        float escala = Mathf.Max(ancho / anchoImagen, alto / altoImagen);

        transform.localScale = new Vector3(escala, escala, 1f);
    }
}
