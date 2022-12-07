using UnityEngine;
using System.Collections;

public class CameraToWorld : MonoBehaviour {

	private Camera mainCamera;

	void Start () {
		mainCamera = this.GetComponent<Camera>();
	}
	
	void OnPreCull(){
		Shader.SetGlobalMatrix("_Camera2World", mainCamera.cameraToWorldMatrix);
	}
}
