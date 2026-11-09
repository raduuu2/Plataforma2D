using UnityEngine;

// Música de fondo en bucle
public class Musica : MonoBehaviour
{
    public AudioClip cancion;

    // Volumen bajito para que se oigan bien los efectos (de 0 a 1)
    public float volumen = 0.3f;

    private AudioSource fuente;

    void Start()
    {
        fuente = gameObject.AddComponent<AudioSource>();

        fuente.clip = cancion;

        fuente.loop = true;

        fuente.volume = volumen;

        fuente.Play();
    }

    void Update()
    {
        // Si has perdido todas las vidas paramos la música para que se oiga el Game Over
        // (en el menú no hay GameManager, por eso miramos que no sea null)
        if (GameManager.Instance != null && GameManager.Instance.lives <= 0 && fuente.isPlaying)
        {
            fuente.Stop();
        }
    }
}
