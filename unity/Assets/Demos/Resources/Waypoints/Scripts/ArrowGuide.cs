using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGuide : MonoBehaviour
{
    public float LerpDistance = 1;
    public float LerpDuration = 1;

    private float LerpValue;
    private float LerpRate;

    private Transform GuideTransform;

    void Start() {
        GuideTransform = transform.GetChild(0);
        LerpValue = 0;
        LerpRate = 1 / LerpDuration;
    }

    void FixedUpdate() {
        Vector3 startPos = Vector3.zero;
        
        LerpValue += Time.deltaTime * LerpRate;

        if (LerpValue >= 1) {
            GuideTransform.localPosition = startPos;
            LerpValue = 0;
        } else {
            GuideTransform.localPosition = new Vector3(
                startPos.x,
                startPos.y,
                startPos.z + LerpValue * LerpDistance - 0.5f
            );
        }
    }
}
