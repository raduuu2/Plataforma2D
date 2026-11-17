using UnityEngine;

// Bola de fuego que lanza el demonio
public class Proyectil : MonoBehaviour
{
    public float velocidad = 5f;

    // Hacia dónde va (la pone el demonio al dispararla)
    public Vector3 direccion;

    // Segundos que dura si no choca con nada
    public float duracion = 4f;

    // Tamaño del circulito con el que mira si choca con una pared
    public float radioChoque = 0.15f;

    // Efectos (los mismos que usan las monedas y los power-ups)
    public GameObject chispaPrefab;

    public GameObject aroPrefab;

    // Cada cuánto suelta una chispa de la estela
    public float tiempoEntreChispas = 0.03f;

    // Colores del fuego
    public Color colorClaro = new Color(1f, 0.9f, 0.3f);

    public Color colorOscuro = new Color(1f, 0.3f, 0.1f);

    private float tiempo = 0f;

    private float contadorChispas = 0f;

    private Vector3 escalaInicial;

    void Start()
    {
        escalaInicial = transform.localScale;

        // Que mire hacia donde va
        transform.right = direccion;
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        transform.position += direccion * velocidad * Time.deltaTime;

        // ¿Choca con una pared o una plataforma?
        // (miramos qué hay en un circulito, como el ataque del caballero)
        Collider2D[] toques = Physics2D.OverlapCircleAll(transform.position, radioChoque);

        foreach (Collider2D toque in toques)
        {
            // Paredes, suelo y plataformas llevan la etiqueta "Suelo"
            if (toque.CompareTag("Suelo"))
            {
                Explotar(8);

                return;
            }
        }

        // Se estira y encoge rápido, como una llama
        float estirar = 1f + Mathf.Sin(tiempo * 30f) * 0.12f;

        transform.localScale = new Vector3(escalaInicial.x * estirar, escalaInicial.y, escalaInicial.z);

        // Estela de chispas
        contadorChispas -= Time.deltaTime;

        if (contadorChispas <= 0)
        {
            contadorChispas = tiempoEntreChispas;

            SoltarChispa();
        }

        // Si se acaba su tiempo, se apaga
        if (tiempo >= duracion)
        {
            Explotar(4);
        }
    }

    // Una chispa que sale por detrás de la bola
    void SoltarChispa()
    {
        if (chispaPrefab == null)
        {
            return;
        }

        Vector3 cola = transform.position - direccion * 0.25f;

        GameObject chispa = Instantiate(chispaPrefab, cola, Quaternion.identity);

        // Cada chispa de un tamaño
        chispa.transform.localScale = Vector3.one * Random.Range(0.12f, 0.28f);

        // Va hacia atrás y un poco hacia arriba (el fuego sube)
        chispa.GetComponent<Chispa>().velocidad = -direccion * Random.Range(0.5f, 1.5f) + Vector3.up * Random.Range(0.2f, 0.8f);

        chispa.GetComponent<Chispa>().duracion = Random.Range(0.2f, 0.4f);

        // Unas más amarillas y otras más rojas
        chispa.GetComponent<SpriteRenderer>().color = Color.Lerp(colorClaro, colorOscuro, Random.value);
    }

    // Revienta en chispas (y un aro si es un choque fuerte) y desaparece
    void Explotar(int cantidadChispas)
    {
        if (chispaPrefab != null)
        {
            Chispa.Explosion(chispaPrefab, transform.position, colorOscuro, cantidadChispas);
        }

        if (aroPrefab != null && cantidadChispas > 4)
        {
            GameObject aro = Instantiate(aroPrefab, transform.position, Quaternion.identity);

            aro.GetComponent<Aro>().tamanoFinal = 1.2f;

            aro.GetComponent<Aro>().duracion = 0.25f;

            aro.GetComponent<SpriteRenderer>().color = colorClaro;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().RecibirGolpe();

            Explotar(10);
        }
    }
}
