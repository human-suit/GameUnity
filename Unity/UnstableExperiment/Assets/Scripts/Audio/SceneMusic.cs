using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager не найден!");
            return;
        }

        if (music == null)
        {
            Debug.LogError("Музыка не назначена в SceneMusic!");
            return;
        }

        AudioManager.Instance.PlayMusic(music);
    }
}