using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport
{
    public class GuideChooserGame : InteractableGame
    {
        protected override void ChildStart()
        {
        }
        protected override void Grabbed(InteractableItem guide)
        {
            Debug.Log("Grabbed " + guide.name);
            ARGameSession.current.WaypointProvider.GuidePrefabs[0] = guide.GetComponent<WaypointGuide>();

            // if (guide.name == "Arrow Guide" || guide.name == "Sparkles Guide" || guide.name == "Human Guide")
            //  {
            //     WaypointProvider waypointProvider = FindObjectOfType<WaypointProvider>();
            //     if (waypointProvider != null)
            //     {
            //         GameObject guidePrefab = Resources.Load<GameObject>(guide.name);
            //         waypointProvider.SetGuidePrefab(guidePrefab);
            //     }
            //     else
            //     {
            //         Debug.LogError("WaypointProvider not found in scene!");
            //     }
            // }
            guide.transform.parent.gameObject.SetActive(false);
        }
        protected override void Dropped(InteractableItem dropClothes)
        {
        }
        protected override List<InteractionType> AvailableInteractionTypes(){
            return new List<InteractionType>(){InteractionType.Grabbable};
        }
    }
}