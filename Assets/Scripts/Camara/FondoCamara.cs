using UnityEngine;

// El fondo del nivel. Va como hijo de la cámara.
//  - Ocupa toda la pantalla, sea del tamaño que sea.
//  - Parallax: la imagen es algo más grande que la pantalla y baja despacio mientras subes.
//    Al empezar se ven los tejados del castillo; en la cima, la luna.
//  - La luna brilla cada vez más cuanto más alto estás.
//  - Crea las luciérnagas y las motas de polvo.
public class FondoCamara : MonoBehaviour
{
    // Cuánto más grande que la pantalla es la imagen (más grande = se mueve más)
    public float margen = 1.3f;

    // Brillo de la luna (posición dentro de la imagen)
    public GameObject brilloLunaPrefab;

    public Vector2 posicionLuna = new Vector2(-4f, 3.65f);

    // Partículas
    public GameObject luciernagaPrefab;

    public int numLuciernagas = 20;

    public GameObject polvoPrefab;

    public int numPolvo = 25;

    private Camera cam;

    private SpriteRenderer sr;

    private SpriteRenderer brilloLuna;

    // Altura de la cámara al empezar y altura de la meta
    private float yInicio;

    private float yCima;

    void Start()
    {
        cam = Camera.main;

        sr = GetComponent<SpriteRenderer>();

        yInicio = cam.transform.position.y;

        // La cima es donde está la meta (en el menú no hay, así que el fondo no se mueve)
        Meta meta = FindAnyObjectByType<Meta>();

        if (meta != null)
        {
            yCima = meta.transform.position.y;
        }
        else
        {
            yCima = yInicio;
        }

        // El brillo de la luna es hijo del fondo: se mueve y se escala con él
        GameObject luna = Instantiate(brilloLunaPrefab, transform);

        luna.transform.localPosition = new Vector3(posicionLuna.x, posicionLuna.y, -0.1f);

        brilloLuna = luna.GetComponent<SpriteRenderer>();

        CrearParticulas(luciernagaPrefab, numLuciernagas);

        CrearParticulas(polvoPrefab, numPolvo);
    }

    // Crea partículas en sitios al azar de la pantalla, como hijas de la cámara
    void CrearParticulas(GameObject prefab, int cantidad)
    {
        float mitadAlto = cam.orthographicSize;

        float mitadAncho = mitadAlto * cam.aspect;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject particula = Instantiate(prefab, cam.transform);

            particula.transform.localPosition = new Vector3(Random.Range(-mitadAncho, mitadAncho), Random.Range(-mitadAlto, mitadAlto), 5f);
        }
    }

    void Update()
    {
        // Lo que se ve en pantalla
        float alto = cam.orthographicSize * 2f;

        float ancho = alto * cam.aspect;

        // Tamaño de la imagen
        float anchoImagen = sr.sprite.bounds.size.x;

        float altoImagen = sr.sprite.bounds.size.y;

        // Cogemos la escala más grande para que no quede ningún hueco (y un poco más para el parallax)
        float escala = Mathf.Max(ancho / anchoImagen, alto / altoImagen) * margen;

        transform.localScale = new Vector3(escala, escala, 1f);

        // Cuánto hemos subido: 0 al empezar y 1 en la cima
        float progreso = 0f;

        if (yCima > yInicio)
        {
            progreso = Mathf.Clamp01((cam.transform.position.y - yInicio) / (yCima - yInicio));
        }

        // Parallax: lo que sobra de imagen por arriba y por abajo lo vamos recorriendo al subir
        float sobra = (altoImagen * escala - alto) / 2f;

        float y = Mathf.Lerp(sobra, -sobra, progreso);

        transform.localPosition = new Vector3(0f, y, transform.localPosition.z);

        // La luna brilla más cuanto más alto
        Color color = brilloLuna.color;

        color.a = Mathf.Lerp(0.15f, 0.6f, progreso);

        brilloLuna.color = color;
    }
}
