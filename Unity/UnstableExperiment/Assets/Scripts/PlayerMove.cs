using UnityEngine;

/// <summary>
/// Шаг 1 — ходьба WASD.
/// Вешается на персонажа вместе с Rigidbody2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Tooltip("Скорость ходьбы")]
    public float speed = 4f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D или ←/→
        float y = Input.GetAxisRaw("Vertical");   // W/S или ↑/↓

        var move = new Vector2(x, y);
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        _rb.velocity = move * speed;
    }
}
