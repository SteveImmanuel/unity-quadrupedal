using System;
using System.Collections;
using UnityEngine;

public class StepSolver : MonoBehaviour
{
    [Header("General Config")]
    public float yOffset = .2f;
    public LayerMask terrainLayer;
    public Transform[] feetIKTarget;

    [Header("Foot Config")]
    public float maxFootHingeRotation = 60;

    [Header("Movement Config")]
    public MovementType[] movementTypes;

    private bool isMoving;
    private int moveTypeIdx;
    private int iteratorFootIdx;
    private bool[] isStepComplete;

    private void Awake()
    {
        isStepComplete = new bool[] { true, true, true, true};
    }

    private void Start()
    {
        iteratorFootIdx = 0;
        moveTypeIdx = 0;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            isMoving = true;
            if (Array.TrueForAll(isStepComplete, e => e))
            {
                StartCoroutine(SmoothMove());
            }
        } else
        {
            isMoving = false;
        }
    }

    private RaycastHit RayCastDown(Vector3 pos, float maxDistance)
    {
        Ray downRay = new Ray(pos, Vector3.down);
        Physics.Raycast(downRay, out RaycastHit info, maxDistance, terrainLayer);
        return info;
    }

    private Vector3 GetNextTargetPos(Transform curPos, float stepSize, float maxHeightLift)
    {
        Vector3 source = curPos.position + curPos.forward * stepSize + curPos.up * maxHeightLift * 2; // 2 is safe value so that the initial pos for raycast is always above the ground
        RaycastHit info = RayCastDown(source, maxHeightLift * 5); // 5 is to make sure raycast always hits the ground if it exists
        Vector3 target = info.point;
        target += curPos.up * yOffset; // add offset because the IK target position is slightly above the ground to accomodate the mesh
        return target;
    }

    private void SolveAim(Transform foot, float maxHeightLift)
    {
        RaycastHit info = RayCastDown(foot.position, maxHeightLift * 5);
        Vector3 normal = info.normal;
        Vector3 restDirection = Vector3.Cross(foot.right, normal);
        float distanceSqr = Vector3.SqrMagnitude(info.point - foot.position);

        Quaternion liftRotation = Quaternion.Euler(maxFootHingeRotation, 0, 0);
        Quaternion restRotation = Quaternion.LookRotation(restDirection, Vector3.up);
        foot.rotation = Quaternion.Lerp(restRotation, liftRotation, distanceSqr / (maxHeightLift * maxHeightLift));
    }

    private IEnumerator SmoothMove()
    {
        MovementType movementType = movementTypes[moveTypeIdx];

        int footIdx = movementType.stepOrder[iteratorFootIdx];
        int nextFootIdx = movementType.stepOrder[(iteratorFootIdx + 1) % 4];
        Vector3 targetPos = GetNextTargetPos(feetIKTarget[footIdx], movementType.stepSize, movementType.maxHeightLift);

        float elapsedTime = 0;
        Vector3 originalPos = feetIKTarget[footIdx].position;
        bool hasInvokedNextStep = false;
        isStepComplete[footIdx] = false;

        while (elapsedTime < movementType.stepDuration)
        {
            float currentStep = elapsedTime / movementType.stepDuration;

            Vector3 newPos = Vector3.Lerp(originalPos, targetPos, currentStep);
            float curveEval = movementType.movementCurve.Evaluate(currentStep);
            newPos.y += curveEval * movementType.maxHeightLift;
            feetIKTarget[footIdx].position = newPos;
            SolveAim(feetIKTarget[footIdx], movementType.maxHeightLift);

            if (currentStep >= movementType.delayBeforeStep[nextFootIdx] && !hasInvokedNextStep && isMoving)
            {
                hasInvokedNextStep = true;
                iteratorFootIdx = (iteratorFootIdx + 1) % 4;
                StartCoroutine(SmoothMove());
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        feetIKTarget[footIdx].position = targetPos;
        isStepComplete[footIdx] = true;

        if (!hasInvokedNextStep)
        {
            iteratorFootIdx = (iteratorFootIdx + 1) % 4;
        }
    }
}
