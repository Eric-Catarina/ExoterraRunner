using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public AnimationCurve speedCurve;
    public static float globalSpeedMultiplier = 1f;

    void Update(){
        globalSpeedMultiplier = speedCurve.Evaluate(Time.time);
    }
}
