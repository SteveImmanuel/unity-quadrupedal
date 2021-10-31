using UnityEngine;

public class FeetNormalSolver : MonoBehaviour
{
    public LayerMask terrainLayer;
    public float maxRaycastDistance;
    public float yOffset;
    public Transform[] feetNormalPos;

    private Vector3[] oriLocalPos;
    private Vector3[] targetPos;
    private MoveController moveController;
    private StepSolverV2 stepSolver;
    private Vector3 targetDirection;

    private void Awake()
    {
        targetPos = new Vector3[4];
        oriLocalPos = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            oriLocalPos[i] = feetNormalPos[i].localPosition;
        }
    }

    void Start()
    {
        moveController = GetComponent<MoveController>();
        stepSolver = GetComponent<StepSolverV2>();
    }

    void FixedUpdate()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i > 1) // for rear legs
            {
                targetDirection = moveController.targetDirectionY - moveController.targetDirectionX;
            }
            else // for front legs
            {
                targetDirection = moveController.targetDirectionY + moveController.targetDirectionX;
            }

            targetPos[i] = transform.TransformPoint(oriLocalPos[i]) + targetDirection.normalized * stepSolver.moveType.stepSize;

            Ray downRay = new Ray(targetPos[i] + Vector3.up * .5f * maxRaycastDistance, Vector3.down);
            if (Physics.Raycast(downRay, out RaycastHit info, maxRaycastDistance, terrainLayer))
            {
                targetPos[i].y = info.point.y + yOffset;
                feetNormalPos[i].position = targetPos[i];
                feetNormalPos[i].rotation = Quaternion.LookRotation(Vector3.Cross(feetNormalPos[i].right, info.normal), info.normal);
            }
        }
    }
}
