using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicCellMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private float _maxSpeed = 1f;
    [SerializeField] private float _acceleration = 20f;
    [SerializeField] private float _updateInterval = 0.1f; // Time interval in seconds
    [SerializeField] private LayerMask _rayLayerMask; // To filter the objects hit by the ray
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private DetectionCircle detectionCircle;
    Custom.VectorPolar[] _directionVectors = new Custom.VectorPolar[16];
    private Vector2 _moveVector;
    private Vector2 _perlinInitPos;
    private float _perlinOffset = 0f;
    private float _timeSinceLastUpdate;

    private GameObject _target = null;
    private LinkedList<GameObject> _enemy = new LinkedList<GameObject>();

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _moveVector = Vector2.zero;
        _perlinInitPos = new Vector2(UnityEngine.Random.value * 10000, UnityEngine.Random.value * 10000);
        _timeSinceLastUpdate = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        CreateDirectionVectors();
        detectionCircle = GetComponentInChildren<DetectionCircle>();
        if (detectionCircle != null)
        {
            // Subscribe to detection events
            detectionCircle.OnObjectDetected += HandleObjectDetected;
            detectionCircle.OnObjectLost += HandleObjectLost;
        }
    }
    private void FixedUpdate()
    {
        _timeSinceLastUpdate += Time.fixedDeltaTime;

        // Run logic only if the specified time interval has passed
        if (_timeSinceLastUpdate >= _updateInterval)
        {
            _timeSinceLastUpdate = 0f; // Reset timer
            _moveVector = DecisionVector();
            Move(_moveVector);
        }

        _sprite.flipX = _moveVector.x < 0f;
    }

    private Vector2 DecisionVector()
    {
        //Calculates the vector with the largest magnitude and returns it
        //Magnitude of the vector indicates the desire the cell want to go that direction
        ResetDirectionVectors();
        Vector2 maxVector = Vector2.zero;
        Vector2 idleVector = IdleVector(_perlinInitPos);
        Vector2 wallVector = WallVector();
        Vector2 preyVector = PreyVector();
        Vector2 predVector = PredVector();
        for (int i = 0; i < 16; i++)
        {         
            float idleDesire = Custom.Dot(idleVector, _directionVectors[i].Direction);
            float wallDesire = Custom.Dot(wallVector, _directionVectors[i].Direction);
            float preyDesire = Custom.Dot(preyVector, _directionVectors[i].Direction);
            float predDesire = Custom.Dot(predVector, _directionVectors[i].Direction);

            _directionVectors[i].SetMagnitude(idleDesire + preyDesire * 3 - predDesire * 5 + wallDesire * 10);
        }
        for (int i = 0; i < 16 ; i++)
        {
            Vector2 temp = _directionVectors[i].Direction * _directionVectors[i].Magnitude;
            Debug.DrawRay(_rb.position, temp, Color.green, _updateInterval * 1.1f);
            if (_directionVectors[i].Magnitude > maxVector.magnitude)
                maxVector = temp;
        }
        return maxVector.normalized;
    }
    private void CreateDirectionVectors()
    {
        //Creates 16-Vector to unit vectors
        float angleIncrement = 2 * Mathf.PI / 16;

        for (int i = 0; i < 16; i++)
        {
            float angle = i * angleIncrement;
            _directionVectors[i] = new Custom.VectorPolar(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        }
    }

    private void ResetDirectionVectors()
    {
        for (int i = 0; i < 16; i++)
        {
            _directionVectors[i].SetMagnitude(1f);
        }
    }
    private Vector2 IdleVector(Vector2 _perlin_init_pos)
    {
        //Generates random smooth vector for idle
        Vector2 _currentDir = _rb.linearVelocity.normalized;
        Vector2 _moveVector = Vector2.zero;
        _perlinOffset += 0.1f;
        _moveVector.x = Mathf.PerlinNoise(_perlinOffset, _perlin_init_pos.x) - 0.47f;
        _moveVector.y = Mathf.PerlinNoise(_perlinOffset, _perlin_init_pos.y) - 0.47f;
        _moveVector = (_moveVector.normalized + _currentDir * 2).normalized;
        // creates tendency to move in the same direction
        Debug.DrawRay(_rb.position, _moveVector * 5, Color.white, _updateInterval * 1.1f);
        return _moveVector;
    }

    private Vector2 WallVector()
    {
        Vector2 sumVector = Vector2.zero;
        for (int i = 0; i < 16; i++)
        {
            Vector2 tempVector = _directionVectors[i].Vector2();
            if (Physics2D.Raycast(_rb.position, tempVector, 3, _rayLayerMask)) //Wall Detection
            {
   
                tempVector *= 0;
            }
            sumVector += tempVector;
        }
        if (sumVector.magnitude > 0.05)
            Debug.DrawRay(_rb.position, sumVector.normalized * 5, Color.white, _updateInterval * 1.1f);
        return sumVector.normalized;
    }

    private Vector2 PreyVector()
    {
        if (_target != null)
        {
            Vector2 preyVector =  (_target.transform.position - _rb.transform.position).normalized;
            Debug.DrawRay(_rb.position,  preyVector * 5, Color.white, _updateInterval * 1.1f);
            return preyVector;
        }
        return Vector2.zero;
    }

    private Vector2 PredVector()
    {
        Vector2 sumVector = Vector2.zero;
        foreach (GameObject predator in _enemy)
        {
            Debug.Log($"{this.name} is running from {predator.name}");
            Vector2 predVector = (predator.transform.position - _rb.transform.position).normalized;
            Debug.DrawRay(_rb.position, predVector, Color.red, _updateInterval * 1.1f);
            sumVector += predVector;
        }
        if (sumVector.magnitude > 0.05)
            Debug.DrawRay(_rb.position, sumVector.normalized * 5, Color.red, _updateInterval * 1.1f);
        return sumVector.normalized;
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
    private void HandleObjectDetected(GameObject detectedObject)
    {
        if (_target == null && detectedObject.CompareTag("Food"))
        {
            _target = detectedObject;
        } else if (_enemy.Count < 5 && detectedObject.CompareTag("Hazard"))
        {
            _enemy.AddFirst(detectedObject);
        }
        Debug.Log($"{this.name} detected {detectedObject.name}");
    }

    private void HandleObjectLost(GameObject lostObject)
    {
        if (lostObject == _target)
        {
            _target = null;
        } else if (lostObject.CompareTag("Hazard"))
        {
            _enemy.Remove(lostObject);
        }
        Debug.Log($"{this.name} lost {lostObject.name}");
    }
}
