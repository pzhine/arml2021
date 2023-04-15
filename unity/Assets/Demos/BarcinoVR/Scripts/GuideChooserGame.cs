using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport
{
    public class GuideChooserGame : InteractableGame
    {

        protected override void ChildStart()
        {
            ARGameSession.current.WaypointProvider.enabled = false;

        }
        protected override void Grabbed(InteractableItem guide)
        {
            Debug.Log("[GuideChooserGame] Grabbed " + guide.tag);
            ARGameSession.current.WaypointProvider.enabled = true;
            ARGameSession.current.WaypointProvider.CurrentGuideTag = guide.tag;

            guide.transform.parent.gameObject.SetActive(false);
        }
        protected override void Dropped(InteractableItem dropClothes)
        {
        }
        // protected override List<InteractionType> AvailableInteractionTypes()
        // {
        //     return new List<InteractionType>() { InteractionType.Grabbable };
        // }
    }
}