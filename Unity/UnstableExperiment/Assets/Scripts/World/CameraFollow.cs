using UnityEngine;

namespace UnstableExperiment.World
{
    /// <summary>Follow player — attach to Main Camera at runtime.</summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target { get; set; }
        public float smooth = 8f;
        public Vector3 offset = new(0, 0, -10);

        private void LateUpdate()
        {
            if (Target == null) return;
            var desired = Target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        }
    }
}
