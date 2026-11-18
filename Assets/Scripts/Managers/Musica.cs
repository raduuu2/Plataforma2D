using UnityEngine;

// Música de fondo en bucle
public class Musica : MonoBehaviour
{
    public AudioClip cancion;

    // Volumen bajito para que se oigan bien los efectos (de 0 a 1)
    public float volumen = 0.35f;

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
        // Si has perdido todas las vidas o has llegado a la meta, paramos la música
        // para que se oiga el Game Over o la canción de victoria
        // (en el menú no hay GameManager, por eso miramos que no sea null)
        if (GameManager.Instance != null && (GameManager.Instance.vidas <= 0 || GameManager.Instance.partidaTerminada == true) && fuente.isPlaying)
        {
            fuente.Stop();
        }
    }
}
