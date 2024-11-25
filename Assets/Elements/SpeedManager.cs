using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public AnimationCurve speedCurve;
    public static float globalSpeedMultiplier = 1f, startingSpeed = 1f, relativeSpeed = 1f;
    
    void Start(){
        startingSpeed = speedCurve.Evaluate(0);
        
    }
    

    void Update(){
        globalSpeedMultiplier = speedCurve.Evaluate(Time.time);
        
        relativeSpeed = globalSpeedMultiplier / startingSpeed;
    }
}
