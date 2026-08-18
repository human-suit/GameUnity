using UnityEngine;

/// <summary>
/// idle + walk_up / walk_down / walk_left / walk_right по WASD.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerWalkAnim : MonoBehaviour
{
    public float moveThreshold = 0.05f;

    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _sr;
    private string _current = "";

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (_anim == null || !_anim.isActiveAndEnabled) return;

        var v = _rb.velocity;
        bool moving = v.sqrMagnitude > moveThreshold * moveThreshold;

        if (!moving)
        {
            _sr.flipX = false;
            Play("idle");
            return;
        }

        _sr.flipX = false;
        Play(PickWalkState(v));
    }

    private static string PickWalkState(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return v.x > 0 ? "walk_left" : "walk_right";
        return v.y > 0 ? "walk_up" : "walk_down";
    }

    private void Play(string state)
    {
        if (_current == state) return;
        _anim.Play(state, 0, 0f);
        _current = state;
    }
}
