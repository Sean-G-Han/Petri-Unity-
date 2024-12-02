using UnityEngine;

public class Draggable : MonoBehaviour
{
    //Copied Online
    private Vector3 _offset;
    private bool _isDragging = false;

    private void OnMouseDown()
    {
        // Calculate the offset between mouse position and node position
        _offset = transform.position - GetMouseWorldPosition();
        _isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (_isDragging)
        {
            // Update position to follow the mouse
            transform.position = GetMouseWorldPosition() + _offset;
        }
    }

    private void OnMouseUp()
    {
        // Stop dragging
        _isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Convert mouse screen position to world position
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z; // Maintain depth
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}