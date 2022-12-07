// // source: https://github.com/WorldOfZero/UnityVisualizations/blob/master/RevealingShader/MagicLightSource.cs
// using System.Collections.Generic;
// using UnityEngine;

// namespace WorldAsSupport {
//     public class FlashlightRevealer : MonoBehaviour {
//         private List<Material> RevealMaterials;

//         void Start() {
//             Renderer[] renderers = GetComponentsInChildren<Renderer>();
//             RevealMaterials = new List<Material>();
//             foreach(Renderer renderer in renderers) {
//                 foreach(Material material in renderer.materials) {
//                     material.shader = DemoSceneManager.current.RevealingShader;
//                     RevealMaterials.Add(material);
//                 }
//             }
//         }

//         void Update () {
//             Light light = DemoSceneManager.current.FlashlightLight;
//             foreach (Material material in RevealMaterials) {
//                 material.SetVector("_LightPosition", light.transform.position);
//                 material.SetVector("_LightDirection", -light.transform.forward);
//                 material.SetFloat("_LightAngle", light.spotAngle);
//             }
//         }
//     }
// }