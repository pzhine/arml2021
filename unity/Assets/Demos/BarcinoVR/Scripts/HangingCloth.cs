using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingCloth : MonoBehaviour
{
    [Range(0, 30)] public float WindUpdateFrequency = 3;
    [Range(0, 5)] public float WindForce = 1.5f;

    private float LastWindUpdateTime;

    private Cloth Cloth;
    // Start is called before the first frame update
    void Start() {
        ClothSkinningCoefficient[] newConstraints = this.GetComponent<Cloth>().coefficients;
        for(int i=0;i<newConstraints.Length;i++){
            newConstraints[i].maxDistance = 10.0f;
        }
        for(int i=46;i<47;i++){
            newConstraints[i].maxDistance = 0.0f;
        }
        for(int i=52;i<53;i++){
            newConstraints[i].maxDistance = 0.0f;
        }
        Cloth = this.GetComponent<Cloth>();
        Cloth.coefficients = newConstraints;
    }

    void FixedUpdate() {
        if (Time.fixedTime - LastWindUpdateTime > WindUpdateFrequency) {
            LastWindUpdateTime = Time.fixedTime;
            float min = (WindForce - WindForce / 2);
            float max = (WindForce + WindForce / 2);
            float windSpeedX = Random.Range(min, max);
            float windSpeedY = Random.Range(min, max);
            float windSpeedZ = Random.Range(min, max);
            Cloth.externalAcceleration = new Vector3(windSpeedX, windSpeedY, windSpeedZ);
        }

    }
}
