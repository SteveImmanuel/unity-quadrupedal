using System;
using System.Collections;
using UnityEngine;

public class StepSolverV2 : MonoBehaviour
{
    [Header("General Config")]
    public float yOffset = .2f;
    public LayerMask terrainLayer;
    public Transform[] feetIKTarget;
    public Transform[] feetNormalPos;

    [Header("Foot Config")]
    public float maxFootHingeRotation = 60;

    [Header("Movement Config")]
    public MovementType[] movementTypes;

    private MovementType moveType;
    private int iteratorFootIdx;
    private bool[] isStepComplete;
    private Vector3[] targetPos;
    private Quaternion[] targetRot;

    private void Awake()
    {
        isStepComplete = new bool[] { true, true, true, true };
        targetPos = new Vector3[4];
        targetRot = new Quaternion[4];
        for (int i = 0; i < 4; i++)
        {
            targetPos[i] = feetNormalPos[i].position;
            targetRot[i] = feetNormalPos[i].rotation;
        }
    }

    private void Start()
    {
        iteratorFootIdx = 0;
        moveType = movementTypes[0];
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            int footIdx = moveType.stepOrder[iteratorFootIdx];
            if (isStepComplete[i])
            {
                if (Vector3.SqrMagnitude(targetPos[footIdx] - feetNormalPos[footIdx].position) > Mathf.Pow(moveType.stepSize, 2) && i == footIdx)
                {
                    StartCoroutine(SmoothMove());
                }
                else
                {
                    feetIKTarget[i].position = targetPos[i];
                    feetIKTarget[i].rotation = targetRot[i];
                }
            }
        }


    }

    private RaycastHit RayCastDown(Vector3 pos, float maxDistance)
    {
        Ray downRay = new Ray(pos, Vector3.down);
        Physics.Raycast(downRay, out RaycastHit info, maxDistance, terrainLayer);
        return info;
    }

    //private void SolveAim(Transform foot, Quaternion targetRot, float maxHeightLift)
    //{
    //    RaycastHit info = RayCastDown(foot.position, maxHeightLift * 5);
    //    Vector3 normal = info.normal;
    //    float distanceSqr = Vector3.SqrMagnitude(info.point - foot.position);

    //    Quaternion liftRotation = Quaternion.Euler(maxFootHingeRotation, 0, 0);
    //    foot.rotation = Quaternion.Lerp(targetRot, liftRotation, distanceSqr / (maxHeightLift * maxHeightLift));
    //}

    private IEnumerator SmoothMove()
    {
        int footIdx = moveType.stepOrder[iteratorFootIdx];
        int nextFootIdx = moveType.stepOrder[(iteratorFootIdx + 1) % 4];
        isStepComplete[footIdx] = false;
        //targetPos[footIdx] = 2 * feetNormalPos[footIdx].position - targetPos[footIdx];
        targetPos[footIdx] = feetNormalPos[footIdx].position;
        targetRot[footIdx] = feetNormalPos[footIdx].rotation;

        float elapsedTime = 0;
        Vector3 originalPos = feetIKTarget[footIdx].position;
        Quaternion originalRot = feetIKTarget[footIdx].rotation;
        bool hasInvokedNextStep = false;

        while (elapsedTime < moveType.stepDuration)
        {
            float currentStep = elapsedTime / moveType.stepDuration;

            Vector3 newPos = Vector3.Lerp(originalPos, targetPos[footIdx], currentStep);
            Quaternion newRot = Quaternion.Lerp(originalRot, targetRot[footIdx], currentStep);

            float curveEval = moveType.movementCurve.Evaluate(currentStep);
            newPos.y += curveEval * moveType.maxHeightLift;
            feetIKTarget[footIdx].position = newPos;
            feetIKTarget[footIdx].rotation = newRot;

            //SolveAim(feetIKTarget[footIdx], moveType.maxHeightLift);

            if (currentStep >= moveType.delayBeforeStep[nextFootIdx] && !hasInvokedNextStep)
            {
                hasInvokedNextStep = true;
                iteratorFootIdx = (iteratorFootIdx + 1) % 4;
                //StartCoroutine(SmoothMove());
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        feetIKTarget[footIdx].position = targetPos[footIdx];
        isStepComplete[footIdx] = true;

        if (!hasInvokedNextStep)
        {
            iteratorFootIdx = (iteratorFootIdx + 1) % 4;
        }
    }
}
