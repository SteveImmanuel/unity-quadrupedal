using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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
    public MultiPositionConstraint bodyContraint;

    private int moveTypeIdx;

    private Vector3[] targetPos = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

    private void Awake()
    {
        targetPos = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    }

    private void Start()
    {
        StartWalkSequence();
    }

    void Update()
    {
    }

    private void StartWalkSequence()
    {
        moveTypeIdx = (int)MoveTypeIndex.WALK;

        StartCoroutine(SmoothMove((int)FootIndex.REAR_LEFT, moveTypeIdx, 0.2f, () =>
        {
            StartCoroutine(SmoothMove((int)FootIndex.FRONT_LEFT, moveTypeIdx, 0.9f, () =>
            {
                StartCoroutine(SmoothMove((int)FootIndex.REAR_RIGHT, moveTypeIdx, 0.2f, () =>
                {
                    StartCoroutine(SmoothMove((int)FootIndex.FRONT_RIGHT, moveTypeIdx, 0.9f, () =>
                    {
                        StartWalkSequence();
                    }));
                }));
            }));
        }));
    }

    private Vector3 GetNextTargetPos(Transform curPos, float stepSize, float maxHeightLift)
    {
        Vector3 source = curPos.position + curPos.forward * stepSize;
        source.y += 0.5f * maxHeightLift;
        RaycastHit info = RayCastDown(source, maxHeightLift * 5);
        Vector3 target = info.point;
        target.y += yOffset;
        return target;
    }

    private RaycastHit RayCastDown(Vector3 pos, float maxDistance)
    {
        Ray downRay = new Ray(pos, Vector3.down);
        Physics.Raycast(downRay, out RaycastHit info, maxDistance, terrainLayer);
        return info;
    }

    private void SolveAim(Transform foot, float maxHeightLift)
    {
        RaycastHit info = RayCastDown(foot.position, 5 * maxHeightLift);
        Vector3 normal = info.normal;
        Vector3 restDirection = Vector3.Cross(foot.right, normal);
        float distanceSqr = Vector3.SqrMagnitude(info.point - foot.position);

        Quaternion liftRotation = Quaternion.Euler(maxFootHingeRotation, 0, 0);
        Quaternion restRotation = Quaternion.LookRotation(restDirection, Vector3.up);
        foot.rotation = Quaternion.Lerp(restRotation, liftRotation, distanceSqr / (maxHeightLift * maxHeightLift));
    }

    private IEnumerator SmoothMove(int footIdx, int moveTypeIdx, float completionBeforeCallback, Action callback)
    {
        MovementType movementType = movementTypes[moveTypeIdx];
        targetPos[footIdx] = GetNextTargetPos(feetIKTarget[footIdx], movementType.stepSize, movementType.maxHeightLift);

        float elapsedTime = 0;
        float currentStep = 0;
        Vector3 originalPos = feetIKTarget[footIdx].position;
        bool hasInvokedCallback = false;

        while (elapsedTime < movementType.stepDuration)
        {

            currentStep = elapsedTime / movementType.stepDuration;
            elapsedTime += Time.deltaTime;

            Vector3 newPos = Vector3.Lerp(originalPos, targetPos[footIdx], currentStep);
            float curveEval = movementType.movementCurve.Evaluate(currentStep);
            newPos.y += curveEval * movementType.maxHeightLift;
            feetIKTarget[footIdx].position = newPos;
            SolveAim(feetIKTarget[footIdx], movementType.maxHeightLift);

            if (currentStep >= completionBeforeCallback && !hasInvokedCallback)
            {
                hasInvokedCallback = true;
                callback?.Invoke();
            }

            yield return null;
        }

        feetIKTarget[footIdx].position = targetPos[footIdx];
    }

    private void OnDrawGizmos()
    {
        if (targetPos.Length == 0) targetPos = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        for (int i = 0; i < 4; i++)
        {
            if (targetPos[i] != Vector3.zero)
            {
                Gizmos.DrawSphere(targetPos[i], 0.2f);
            }
        }
    }
}
