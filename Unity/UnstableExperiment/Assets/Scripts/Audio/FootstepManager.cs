using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;

    public float stepInterval = 0.4f;

    private float stepTimer;

    public void HandleMovement(Vector2 movement)
    {
        if (movement.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepSounds.Length == 0)
            return;

        AudioClip clip = footstepSounds[
            Random.Range(0, footstepSounds.Length)
        ];

        audioSource.PlayOneShot(clip);
    }
}
