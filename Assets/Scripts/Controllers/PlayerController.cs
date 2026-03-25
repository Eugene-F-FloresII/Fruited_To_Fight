using UnityEngine;
using UnityEngine.InputSystem;


namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private BoxCollider2D _boxCollider;
        [SerializeField] private InputActionReference _movementInput;
    }
}

