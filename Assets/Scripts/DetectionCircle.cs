using System;
using UnityEngine;

public class DetectionCircle : MonoBehaviour
{
    public event Action<GameObject> OnObjectDetected;
    public event Action<GameObject> OnObjectLost;

    void OnTriggerEnter2D(Collider2D collision)
    {
        OnObjectDetected?.Invoke(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        OnObjectLost?.Invoke(collision.gameObject);
    }
}
