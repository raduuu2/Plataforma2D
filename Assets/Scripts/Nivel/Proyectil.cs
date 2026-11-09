using UnityEngine;

// Bola de fuego que lanza el demonio
public class Proyectil : MonoBehaviour
{
    public float velocidad = 5f;

    // Hacia dónde va (la pone el demonio al dispararla)
    public Vector3 direccion;

    void Start()
    {
        // Que mire hacia donde va
        transform.right = direccion;

        // Si no choca con nada, desaparece a los 4 segundos
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().RecibirGolpe();

            Destroy(gameObject);
        }
    }
}
