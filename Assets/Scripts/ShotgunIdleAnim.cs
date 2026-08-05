using UnityEngine;

public class ShotgunIdleAnim : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float speed = 2f;     
    [SerializeField] private float height = 0.5f;
    private Vector3 startLocalPosition;
    void Start()
    {
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * speed) * height;
        transform.localPosition = new Vector3(startLocalPosition.x, startLocalPosition.y + newY, startLocalPosition.z);
    }
}
