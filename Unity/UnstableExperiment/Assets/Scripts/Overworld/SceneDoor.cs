using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneDoor : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnId;
    [SerializeField] private GameObject prompt;

    private bool playerNearby;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;

        if (prompt != null)
            prompt.SetActive(false);
    }

    private void Update()
    {
        if (ReadPanel.IsOpen || ReadPanel.ClosedThisFrame)
            return;

        if (ReadableSign.AnyNearby)
            return;

        if (!playerNearby || !Input.GetKeyDown(KeyCode.E))
            return;

        SceneSpawnData.TargetSpawnId = targetSpawnId;
        SceneManager.LoadScene(targetScene);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() == null)
            return;

        playerNearby = true;

        if (prompt != null)
            prompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() == null)
            return;

        playerNearby = false;

        if (prompt != null)
            prompt.SetActive(false);
    }
}