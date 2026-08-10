using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float positionSmoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float rotationSpeed = 0.1f; // Speed at which the camera rotates to match the target's rotation

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed);
        transform.position = smoothedPosition;
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotationSpeed);
    }
}
