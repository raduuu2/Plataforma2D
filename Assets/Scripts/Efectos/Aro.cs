using UnityEngine;

// Un aro que crece rápido y se va desvaneciendo.
// Lo crean los power-ups al cogerlos.
public class Aro : MonoBehaviour
{
    public float tamanoFinal = 3f;

    public float duracion = 0.5f;

    private float tiempo = 0f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        // De 0 (recién creado) a 1 (se acaba)
        float progreso = tiempo / duracion;

        transform.localScale = Vector3.one * tamanoFinal * progreso;

        Color color = sr.color;

        color.a = 1f - progreso;

        sr.color = color;

        if (tiempo >= duracion)
        {
            Destroy(gameObject);
        }
    }
}
