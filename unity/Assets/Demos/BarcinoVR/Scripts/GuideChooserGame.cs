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

            // HumanGuide hg = guide.GetComponent<HumanGuide>();
            // if (hg != null){
            //     hg.enabled = true;
            // }

            // ArrowGuide ag = guide.GetComponent<ArrowGuide>();
            // if (ag != null){
            //     ag.enabled = true;
            // }

            // WaypointGuide wg = guide.GetComponent<WaypointGuide>();
            // if (wg != null && guide.gameObject.tag != "SparkleGuide"){
            //     wg.enabled = true;
            // }

        }
        protected override void Dropped(InteractableItem dropClothes)
        {
        }
        protected override List<InteractionType> AvailableInteractionTypes()
        {
            return new List<InteractionType>() { InteractionType.Grabbable };
        }
    }
}