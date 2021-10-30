using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    public float speed;
    public float smoothDampTime;
    public float rotationSpeed;
    public LayerMask terrainLayer;
    public float maxRaycastDistance;

    public Transform cam;

    private Rigidbody rb;
    private Vector3 refVelocity;

    private void Awake()
    {
        refVelocity = Vector3.zero;
        rb = GetComponent<Rigidbody>();
    }
    private RaycastHit RayCastDown(Vector3 pos, float maxDistance)
    {
        Ray downRay = new Ray(pos, Vector3.down);
        Physics.Raycast(downRay, out RaycastHit info, maxDistance, terrainLayer);
        return info;
    }

    void Update()
    {
        float xMove = Input.GetAxisRaw("Horizontal");
        float yMove = Input.GetAxisRaw("Vertical");

        RaycastHit info = RayCastDown(cam.position, maxRaycastDistance);
        Vector3 forward = Vector3.Cross(cam.right, info.normal);
        Vector3 targetRotationEuler = forward * yMove + cam.right * xMove;

        if (targetRotationEuler != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetRotationEuler);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            rb.velocity = Vector3.SmoothDamp(rb.velocity, transform.forward * speed, ref refVelocity, smoothDampTime);
        }

    }
}
