using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 6f;

    public float jumpForce = 14f;

    public bool isGrounded = false;

    public bool isDead = false;

    public Vector3 respawnPoint;

    // Los power-ups cambian estos valores (1 = normal)
    public float speedMultiplier = 1f;

    public float jumpMultiplier = 1f;

    private Rigidbody2D rb;

    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.freezeRotation = true; // Que el personaje no se caiga de lado al chocar

        animator = GetComponent<Animator>();

        respawnPoint = transform.position;
    }

    void Update()
    {
        if (isDead == true)
        {
            return;
        }

        // Movimiento lateral
        float dir = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dir = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dir = 1f;
        }

        rb.linearVelocity = new Vector2(dir * speed * speedMultiplier, rb.linearVelocity.y);

        // Girar al personaje hacia donde camina
        if (dir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dir > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * jumpMultiplier);

            isGrounded = false;
        }

        // Animaciones
        if (animator != null)
        {
            animator.SetFloat("movement", Mathf.Abs(dir));

            animator.SetBool("enSuelo", isGrounded);

            animator.SetFloat("velocidadY", rb.linearVelocity.y);
        }
    }

    // Mientras toca algo, miramos si ese algo está DEBAJO de los pies
    // (normal.y > 0.5 significa que la superficie mira hacia arriba, o sea, es suelo y no una pared)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    // Lo llaman la cámara (si te caes), los enemigos o la lava
    public void Die()
    {
        if (isDead == true)
        {
            return;
        }

        isDead = true;

        rb.linearVelocity = Vector2.zero;

        rb.simulated = false;

        GameManager.Instance.PlayerDied();

        if (GameManager.Instance.gameOver == false)
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1f);

        transform.position = respawnPoint;

        rb.simulated = true;

        rb.linearVelocity = Vector2.zero;

        speedMultiplier = 1f;

        jumpMultiplier = 1f;

        isDead = false;
    }
}
