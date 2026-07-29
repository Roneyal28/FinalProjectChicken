using System;
using Unity.Mathematics;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject target;

   [SerializeField] float speed = 1f; 
   [SerializeField] float minX;
   [SerializeField] float maxX;
   [SerializeField] float minY;
   [SerializeField] float maxY;
   Vector3 targetCamPos;

    private float CamZ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        CamZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        targetCamPos = target.transform.position;
        targetCamPos.x = math.clamp(targetCamPos.x, minX, maxX);
        targetCamPos.y = math.clamp(targetCamPos.y, minY, maxY);*/
        transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime * speed);
        transform.position = new Vector3(transform.position.x, transform.position.y, CamZ);
    }
}
