using UnityEngine;

namespace UnstableExperiment.World
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        public float speed = 4f;
        private Rigidbody2D _rb;
        private RoomManager _rooms;
        private Combat.CombatManager _combat;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rooms = GetComponentInParent<RoomManager>();
            if (_rooms == null)
                _rooms = FindObjectOfType<RoomManager>();
            _combat = FindObjectOfType<Combat.CombatManager>();
        }

        private void Update()
        {
            if (_combat != null && _combat.IsActive) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                _rooms?.TryPickupNearby();
                _rooms?.TryUseNearbyDoor();
            }
        }

        private void FixedUpdate()
        {
            if (_combat != null && _combat.IsActive)
            {
                _rb.velocity = Vector2.zero;
                return;
            }

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();
            _rb.velocity = input * speed;
        }
    }
}
