using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowingBowl : MonoBehaviour
{
    ParticleSystem bowlParticleSystem;
    
    void Start()
    {
        bowlParticleSystem = GetComponentsInChildren<ParticleSystem>()[0];
        bowlParticleSystem.scalingMode = ParticleSystemScalingMode.Hierarchy;
    }
}
