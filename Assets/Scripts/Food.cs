using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Food : MonoBehaviour
{
    private DetectionCircle detectionCircle;
    void Start()
    {
        detectionCircle = GetComponentInChildren<DetectionCircle>();
        if (detectionCircle != null)
        {
            // Subscribe to detection events
            detectionCircle.OnObjectDetected += HandleObjectDetected;
        }
    }

    private void HandleObjectDetected(GameObject detectedObject)
    {
        if (detectedObject.CompareTag("Player"))
        {
            Destroy(this.gameObject);
        }
    }
}
