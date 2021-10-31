using UnityEngine;

public class AlwaysOnTopSurface : MonoBehaviour
{
    public LayerMask terrainLayer;
    public float maxRaycastDistance;
    public float yOffset;

    void Start()
    {

    }

    void FixedUpdate()
    {
        Ray downRay = new Ray(transform.position + Vector3.up * .5f * maxRaycastDistance, Vector3.down);
        if (Physics.Raycast(downRay, out RaycastHit info, maxRaycastDistance, terrainLayer))
        {
            Vector3 newPos = transform.position;
            newPos.y = info.point.y + yOffset;
            transform.position = newPos;
            transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, info.normal));

            //transform.position = transform.up * yOffset;

        }

    }
}
