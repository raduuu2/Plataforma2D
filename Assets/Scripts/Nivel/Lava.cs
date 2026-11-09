using UnityEngine;

public class Lava : MonoBehaviour
{
    public float velocidad = 0.5f;

    public Player player;

    // Al morir, la lava baja hasta esta distancia por debajo del checkpoint
    // (la lava mide 40 de alto, así que 20 es la mitad + 7 de margen)
    public float distanciaRespawn = 27f;

    void Update()
    {
        if (GameManager.Instance.gameOver == true)
        {
            return;
        }

        if (player.isDead == true)
        {
            // Bajamos la lava para que el jugador pueda reaparecer
            float limite = player.respawnPoint.y - distanciaRespawn;

            if (transform.position.y > limite)
            {
                transform.position = new Vector3(transform.position.x, limite, 0f);
            }
        }
        else
        {
            transform.position += Vector3.up * velocidad * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().Die();
        }
    }
}
