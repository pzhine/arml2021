using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When placed on a GameObject, it replaces all child GameObject materials
/// with the SpecularStencilFilter shader. This shader makes the objects
/// visible only through a plane with the PortalDoor shader on it.
/// </summary>
public class PortalObject : MonoBehaviour {
    public Shader PortalModeShader;

    // Start is called before the first frame update
    void Start() {
        MeshRenderer[] meshRenderers = gameObject.transform.GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer m in meshRenderers) {
            m.material.shader = PortalModeShader;
        }
    }
}
