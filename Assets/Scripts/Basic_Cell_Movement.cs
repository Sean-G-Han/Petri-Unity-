using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class Basic_Cell_Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private float _maxSpeed = 7f;
    [SerializeField] private float _acceleration = 70f;
    [SerializeField] private float _updateInterval = 0.1f; // Time interval in seconds
    [SerializeField] private LayerMask _rayLayerMask; // To filter the objects hit by the ray
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
        Vector2 wallVector = Wall_vector();
        for (int i = 0; i < 16; i++)
        {         
            float idleMultiplier = Vector2.Dot(idleVector, _directionVectors[i]);
            float wallMultiplier;
            if (wallVector.magnitude < 0.05f)
                wallMultiplier = 1f;
            else
                wallMultiplier = Vector2.Dot(wallVector, _directionVectors[i]);

            //Only if wall not detected, then calculate desire.
            _directionVectors[i] = (_directionVectors[i] + _directionVectors[i] * idleMultiplier / 2) * wallMultiplier;
        }
        for (int i = 0; i < 16 ; i++)
        {
            Debug.DrawRay(_rb.position, _directionVectors[i] * 3, Color.green, _updateInterval * 1.1f);
            if (_directionVectors[i].magnitude > maxVector.magnitude)
                maxVector = _directionVectors[i];
        }
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

    private Vector2 Wall_vector()
    {
        Vector2 sumVector = Vector2.zero;
        for (int i = 0; i < 16; i++)
        {
            Vector2 tempVector = _directionVectors[i];
            if (Physics2D.Raycast(_rb.position, tempVector, 3, _rayLayerMask)) //Wall Detection
            {
                tempVector *= 0;
            }
            sumVector += tempVector;
        }
        return sumVector;
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
