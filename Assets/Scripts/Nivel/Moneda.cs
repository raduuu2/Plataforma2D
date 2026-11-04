using UnityEngine;

public class Moneda : MonoBehaviour
{
    public int points = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddPoints(points);

            Destroy(gameObject);
        }
    }
}
