using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    public Player player;

    public float offsetY = 1f;

    public float smooth = 5f;

    // Si el jugador baja más que esto por debajo del centro de la cámara, muere
    public float deathDistance = 6f;

    private float maxY;

    void Start()
    {
        maxY = player.transform.position.y + offsetY;
    }

    void LateUpdate()
    {
        // Si ha muerto, la cámara vuelve a la altura del checkpoint
        if (player.isDead == true)
        {
            maxY = player.respawnPoint.y + offsetY;
        }

        // Solo sube cuando el jugador está en el suelo y más alto que antes (nunca baja)
        if (player.isGrounded == true && player.transform.position.y + offsetY > maxY)
        {
            maxY = player.transform.position.y + offsetY;
        }

        float x = Mathf.Lerp(transform.position.x, player.transform.position.x, smooth * Time.deltaTime);

        float y = Mathf.Lerp(transform.position.y, maxY, smooth * Time.deltaTime);

        transform.position = new Vector3(x, y, -10f);

        // ¿Se ha caído por debajo de la pantalla?
        if (player.transform.position.y < transform.position.y - deathDistance)
        {
            player.Die();
        }
    }
}
