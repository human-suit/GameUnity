using UnityEngine;

/// <summary>
/// Шаг 1 — камера следует за игроком.
/// Вешается на Main Camera. В поле Target перетащи Player.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Tooltip("За кем следить — перетащи Player сюда")]
    public Transform target;

    [Tooltip("Насколько плавно камера догоняет")]
    public float smooth = 8f;

    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 wanted = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, wanted, smooth * Time.deltaTime);
    }
}
