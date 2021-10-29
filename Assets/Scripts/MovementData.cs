using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MovementType
{
    public float speed;
    public AnimationCurve movementCurve;
    public float maxHeightLift;
    public float stepSize;
    public float stepDuration;
    public int[] stepOrder;
    public float[] delayBeforeStep; // in percentage
}

public enum MoveTypeIndex
{
    WALK,
}

public enum FootIndex
{
    FRONT_LEFT,
    FRONT_RIGHT,
    REAR_LEFT,
    REAR_RIGHT,
}
