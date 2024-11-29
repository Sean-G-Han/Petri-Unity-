using UnityEngine;
using UnityEngine.InputSystem;

public class Basic_Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private float _maxspeed = 7f;
    [SerializeField] private float _acceleration = 70f;
    [SerializeField] private InputAction player_control;
    private Rigidbody2D _rb;
    private Vector2 _move_vector;

    private void OnEnable()
    {
        player_control.Enable();
    }

    private void OnDisable()
    {
        player_control.Disable();
    }
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _move_vector = player_control.ReadValue<Vector2>().normalized;
    }

    private void FixedUpdate()
    {
        move(_move_vector);
    }

    private void move(Vector2 _move_vector)
    {
        if (_move_vector != Vector2.zero)
        {
            _rb.AddForce(_move_vector * _acceleration, ForceMode2D.Force);

            if (_rb.linearVelocity.magnitude > _maxspeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * _maxspeed;
            }
        }
        else
        {
            _rb.AddForce(-_rb.linearVelocity.normalized * _acceleration, ForceMode2D.Force);
            if (_rb.linearVelocity.magnitude < 0.5)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
