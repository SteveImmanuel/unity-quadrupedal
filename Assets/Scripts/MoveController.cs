using UnityEngine;

public class MoveController : MonoBehaviour
{
    public float speed;
    public float smoothDampTime;
    public float rotationSpeed;
    public LayerMask terrainLayer;
    public float maxRaycastDistance;
    public Transform cam;
    public Transform[] actualFeetPos;
    public float height;
    public float yOffset;
    [Range(0f, 1f)] public float weight;

    [HideInInspector] public Vector3 targetDirectionY;
    [HideInInspector] public Vector3 targetDirectionX;

    private float currentSpeed;
    private Vector3 rawForward;
    private float xMove;
    private float yMove;

    private void Awake()
    {
        currentSpeed = 0;
        targetDirectionY = Vector3.zero;
        targetDirectionX = Vector3.zero;
        rawForward = transform.forward;
    }

    private RaycastHit RayCastDown(Vector3 pos, float maxDistance)
    {
        Ray downRay = new Ray(pos, Vector3.down);
        Physics.Raycast(downRay, out RaycastHit info, maxDistance, terrainLayer);
        return info;
    }

    void Update()
    {
        xMove = Input.GetAxisRaw("Horizontal");
        yMove = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        float targetSpeed = 0;
        RaycastHit camRaycastInfo = RayCastDown(cam.position, maxRaycastDistance);
        Vector3 forward = Vector3.Cross(cam.right, camRaycastInfo.normal);

        targetDirectionY = forward * yMove;
        targetDirectionX = cam.right * xMove;

        if (targetDirectionX + targetDirectionY != Vector3.zero)
        {
            rawForward = targetDirectionX + targetDirectionY;
            targetSpeed = speed;
        }

        RaycastHit bodyRaycastInfo = RayCastDown(transform.position, height * 2);

        Vector3 restPos = bodyRaycastInfo.point + Vector3.up * height;
        Vector3 targetPos = bodyRaycastInfo.point + Vector3.up * (GetAverageDistanceFromGround() - yOffset + height);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speed * Time.fixedDeltaTime);

        transform.SetPositionAndRotation(
            Vector3.Lerp(restPos, targetPos, weight) + currentSpeed * Time.deltaTime * rawForward, 
            Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(rawForward, Vector3.up), rotationSpeed * Time.deltaTime
        ));
    }

    private float GetAverageDistanceFromGround()
    {
        float sum = 0;
        for(int i = 0; i < 4; i++)
        {
            RaycastHit info = RayCastDown(actualFeetPos[i].position, 10);
            sum += info.distance;
        }
        return sum / 4;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + rawForward * 2);
    }
}
