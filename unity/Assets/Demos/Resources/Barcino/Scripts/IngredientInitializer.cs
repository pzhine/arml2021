using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var render in renderers){
            render.material.renderQueue = 3002; // set their renderQueue
        }
    }
}
