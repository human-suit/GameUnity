using UnityEngine;

public static class SceneSpawnData
{
    public static string TargetSpawnId;
}

public class SceneSpawn : MonoBehaviour
{
    [SerializeField] private string spawnId;

    private void Start()
    {
        if (SceneSpawnData.TargetSpawnId != spawnId)
            return;

        PlayerMove player = FindObjectOfType<PlayerMove>();

        if (player == null)
            return;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.position = transform.position;
            body.velocity = Vector2.zero;
        }
        else
        {
            player.transform.position = transform.position;
        }

        SceneSpawnData.TargetSpawnId = null;
        Physics2D.SyncTransforms();
    }
}