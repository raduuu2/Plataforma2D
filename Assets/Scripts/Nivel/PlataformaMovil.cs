using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    // false = se mueve en horizontal, true = en vertical
    public bool vertical = false;

    public float velocidad = 2f;

    // Cuánto se aleja a cada lado de donde empieza
    public float distancia = 3f;

    private Vector3 inicio;

    private int direccion = 1;

    void Start()
    {
        inicio = transform.position;
    }

    void Update()
    {
        if (vertical == false)
        {
            transform.position += Vector3.right * direccion * velocidad * Time.deltaTime;

            if (transform.position.x > inicio.x + distancia)
            {
                direccion = -1;
            }

            if (transform.position.x < inicio.x - distancia)
            {
                direccion = 1;
            }
        }
        else
        {
            transform.position += Vector3.up * direccion * velocidad * Time.deltaTime;

            if (transform.position.y > inicio.y + distancia)
            {
                direccion = -1;
            }

            if (transform.position.y < inicio.y - distancia)
            {
                direccion = 1;
            }
        }
    }

    // Cuando el jugador se sube, lo hacemos hijo de la plataforma para que se mueva con ella
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.transform.position.y > transform.position.y)
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
