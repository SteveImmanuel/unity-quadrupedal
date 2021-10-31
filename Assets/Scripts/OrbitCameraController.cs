using UnityEngine;

[ExecuteInEditMode]
public class OrbitCameraController : MonoBehaviour
{
    public bool debugMode;
    public float smoothTime = .5f;
    public Transform orbitTarget;
    public float orbitRadius = 10;

    [Header("Y Axis")]
    [Range(-1f, 1f)]
    public float yAxisValue;
    public bool invertYAxis;
    public float ySensitivity = 0.1f;
    [Range(0f, 90f)]
    public float maxAngleFromVertical;

    [Header("X Axis")]
    [Range(0f, 359f)]
    public float xAxisValue;
    public bool invertXAxis;
    public float xSensitivity = 4f;

    private Vector3 refVelocity;
    private Vector3 offset;
    private float radiusAtY;

    private void Awake()
    {
        refVelocity = Vector3.zero;
        offset = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (!debugMode || Input.GetButton("Fire2"))
        {
            xAxisValue = (xAxisValue + Input.GetAxis("Mouse X") * xSensitivity * (invertXAxis ? 1 : -1)) % 360;
            yAxisValue += Input.GetAxis("Mouse Y") * ySensitivity * (invertYAxis ? 1 : -1);
        }

        yAxisValue = Mathf.Clamp(yAxisValue, -Mathf.Cos(Mathf.Deg2Rad * maxAngleFromVertical), Mathf.Cos(Mathf.Deg2Rad * maxAngleFromVertical));
        offset.y = yAxisValue * orbitRadius;
        radiusAtY = Mathf.Sqrt(Mathf.Pow(orbitRadius, 2) - Mathf.Pow(offset.y, 2));
        offset.x = Mathf.Cos(Mathf.Deg2Rad * xAxisValue) * radiusAtY;
        offset.z = Mathf.Sin(Mathf.Deg2Rad * xAxisValue) * radiusAtY;

        if (Application.isPlaying)
        {
            transform.position = Vector3.SmoothDamp(transform.position, orbitTarget.position + offset, ref refVelocity, smoothTime);
        }
        else
        {
            transform.position = orbitTarget.position + offset;
        }
        transform.rotation = Quaternion.LookRotation(orbitTarget.position - transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        float maxYAxisValue = Mathf.Cos(Mathf.Deg2Rad * maxAngleFromVertical);
        Vector3 delta = Vector3.zero;
        delta.y = maxYAxisValue * orbitRadius;
        float radius = Mathf.Sqrt(Mathf.Pow(orbitRadius, 2) - Mathf.Pow(delta.y, 2));

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(orbitTarget.position, orbitTarget.up, orbitRadius);
        UnityEditor.Handles.DrawWireDisc(orbitTarget.position + delta, orbitTarget.up, radius);
        UnityEditor.Handles.DrawWireDisc(orbitTarget.position - delta, orbitTarget.up, radius);
    }
}
