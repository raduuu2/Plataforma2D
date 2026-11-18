using UnityEngine;

// Un trocito de niebla que sale por arriba: sube haciendo eses y se va desvaneciendo
public class Jiron : MonoBehaviour
{
    public float duracion = 1.6f;

    public float velocidadSubida = 2f;

    private float tiempo = 0f;

    private float desfase;

    private float alfaInicial;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        alfaInicial = sr.color.a;

        // Para que no hagan todos las eses igual
        desfase = Random.Range(0f, 10f);
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        // Sube haciendo eses
        float x = Mathf.Sin(tiempo * 3f + desfase) * 0.4f;

        transform.position += new Vector3(x, velocidadSubida, 0f) * Time.deltaTime;

        // Cada vez más transparente
        Color color = sr.color;

        color.a = alfaInicial * (1f - tiempo / duracion);

        sr.color = color;

        if (tiempo >= duracion)
        {
            Destroy(gameObject);
        }
    }
}
