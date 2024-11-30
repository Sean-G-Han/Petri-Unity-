using UnityEngine;
using UnityEngine.UIElements;

public class Basic_Cell_Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private float _maxSpeed = 7f;
    [SerializeField] private float _acceleration = 70f;
    [SerializeField] private float _updateInterval = 0.1f; // Time interval in seconds
    [SerializeField] private LayerMask _apple; // To filter the objects hit by the ray
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    Vector2[] _directionVectors = new Vector2[16];
    private Vector2 _moveVector;
    private Vector2 _perlinInitPos;
    private float _perlinOffset = 0f;
    private float _timeSinceLastUpdate;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _moveVector = Vector2.zero;
        _perlinInitPos = new Vector2(Random.value * 10000, Random.value * 10000);
        _timeSinceLastUpdate = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

    }
    private void FixedUpdate()
    {
        _timeSinceLastUpdate += Time.fixedDeltaTime;

        // Run logic only if the specified time interval has passed
        if (_timeSinceLastUpdate >= _updateInterval)
        {
            _timeSinceLastUpdate = 0f; // Reset timer
            _moveVector = Decision_vector();
            Move(_moveVector);
        }

        _sprite.flipX = _moveVector.x < 0f;
    }

    private Vector2 Decision_vector()
    {
        //Calculates the vector with the largest magnitude and returns it
        //Magnitude of the vector indicates the desire the cell want to go that direction
        Reset_direction_vectors();
        Vector2 maxVector = Vector2.zero;
        Vector2 idleVector = Idle_vector(_perlinInitPos);
        for (int i = 0; i < 16; i++)
        {         
            float idleMultiplier = Vector2.Dot(idleVector, _directionVectors[i]);
            float stdMultiplier = 1f;
            if (Physics2D.Raycast(_rb.position, _directionVectors[i], 3, _apple)) //Wall Detection
            {
                //If Wall deteted, draw (DEBUGGING purposes)
                Debug.DrawRay(_rb.position, _directionVectors[i] * 3, Color.red);
                stdMultiplier = 0f;
                //Set neighbours to 0 too
                if (i < 15) _directionVectors[i + 1] *= 0;
                else _directionVectors[0] *= 0;
                if (i > 0) _directionVectors[i - 1] *= 0;
                else _directionVectors[15] *= 0;
            }
            //Only if wall not detected, then calculate desire.
            _directionVectors[i] = (_directionVectors[i] + _directionVectors[i] * idleMultiplier / 100) * stdMultiplier;
        }
        for (int i = 0; i < 16 ; i++)
        {
            if (_directionVectors[i].magnitude > maxVector.magnitude)
                maxVector = _directionVectors[i];
        }
        Debug.DrawRay(_rb.position, maxVector.normalized * 5, Color.white);
        return maxVector;
    }
    private void Reset_direction_vectors()
    {
        //Resets 16-Vector to unit vectors
        float angleIncrement = 2 * Mathf.PI / 16;

        for (int i = 0; i < 16; i++)
        {
            float angle = i * angleIncrement;
            _directionVectors[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
    private Vector2 Idle_vector(Vector2 _perlin_init_pos)
    {
        //Generates random smooth vector for idle
        Vector2 _currentDir = _rb.linearVelocity.normalized;
        Vector2 _moveVector = Vector2.zero;
        _perlinOffset += 0.1f;
        _moveVector.x = Mathf.PerlinNoise(_perlinOffset, _perlin_init_pos.x) - 0.47f;
        _moveVector.y = Mathf.PerlinNoise(_perlinOffset, _perlin_init_pos.y) - 0.47f;
        _moveVector = _moveVector.normalized;
        // creates tendency to move in the same direction
        return (_moveVector + _currentDir * 2).normalized;
    }
    private void Move(Vector2 _move_vector)
    {
        if (_move_vector != Vector2.zero)
        {
            _rb.AddForce(_move_vector * _acceleration, ForceMode2D.Force);

            if (_rb.linearVelocity.magnitude > _maxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * _maxSpeed;
            }
        }
        else
        {
            _rb.AddForce(-_rb.linearVelocity.normalized * _acceleration, ForceMode2D.Force);
            if (_rb.linearVelocity.magnitude < _maxSpeed / 10f) // if velocity is small, just stop it
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
