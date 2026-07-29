using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject target;

    [Header("Follow")]
    [SerializeField] float speed = 1f;

    [Header("Barn Limits")]
    [Tooltip("Left edge of the area the camera can show.")]
    [SerializeField] float minX;
    [Tooltip("Right edge of the area the camera can show.")]
    [SerializeField] float maxX;
    [Tooltip("Bottom edge of the area the camera can show.")]
    [SerializeField] float minY;
    [Tooltip("Top edge of the area the camera can show.")]
    [SerializeField] float maxY;

    Vector3 targetCamPos;

    private Camera followCamera;
    private float camZ;

    void Awake()
    {
        followCamera = GetComponent<Camera>();
        camZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        targetCamPos = target.transform.position;
        ClampCameraPosition(ref targetCamPos);
        targetCamPos.z = camZ;

        Vector3 newCameraPosition = Vector3.Lerp(transform.position, targetCamPos, Time.deltaTime * speed);
        ClampCameraPosition(ref newCameraPosition);
        newCameraPosition.z = camZ;

        transform.position = newCameraPosition;
    }

    private void ClampCameraPosition(ref Vector3 cameraPosition)
    {
        float halfHeight = 0f;
        float halfWidth = 0f;

        if (followCamera != null && followCamera.orthographic)
        {
            halfHeight = followCamera.orthographicSize;
            halfWidth = halfHeight * followCamera.aspect;
        }

        float cameraMinX = minX + halfWidth;
        float cameraMaxX = maxX - halfWidth;
        float cameraMinY = minY + halfHeight;
        float cameraMaxY = maxY - halfHeight;

        if (cameraMinX > cameraMaxX)
        {
            float centerX = (minX + maxX) * 0.5f;
            cameraMinX = centerX;
            cameraMaxX = centerX;
        }

        if (cameraMinY > cameraMaxY)
        {
            float centerY = (minY + maxY) * 0.5f;
            cameraMinY = centerY;
            cameraMaxY = centerY;
        }

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, cameraMinX, cameraMaxX);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, cameraMinY, cameraMaxY);
    }
}
