using UnityEngine;

// Una chispa que sale disparada, se frena, se hace pequeña y desaparece.
// Las crean las monedas y los power-ups al cogerlos.
public class Chispa : MonoBehaviour
{
    public Vector2 velocidad;

    public float duracion = 0.5f;

    private float tiempo = 0f;

    private float tamanoInicial;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        tamanoInicial = transform.localScale.x;
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        // Se mueve y poco a poco se va frenando
        transform.position += (Vector3)velocidad * Time.deltaTime;

        velocidad -= velocidad * 4f * Time.deltaTime;

        // Lo que le queda de vida, de 1 (recién creada) a 0 (se acaba)
        float queda = 1f - tiempo / duracion;

        // Cada vez más pequeña y más transparente
        transform.localScale = Vector3.one * tamanoInicial * queda;

        Color color = sr.color;

        color.a = queda;

        sr.color = color;

        if (tiempo >= duracion)
        {
            Destroy(gameObject);
        }
    }

    // Crea varias chispas en círculo, cada una hacia un lado
    public static void Explosion(GameObject prefab, Vector3 posicion, Color color, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GameObject chispa = Instantiate(prefab, posicion, Quaternion.identity);

            // Repartimos las chispas por la vuelta entera (2 * PI)
            float angulo = i * 2f * Mathf.PI / cantidad;

            Vector2 direccion = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo));

            chispa.GetComponent<Chispa>().velocidad = direccion * Random.Range(2f, 4f);

            chispa.GetComponent<SpriteRenderer>().color = color;
        }
    }
}
