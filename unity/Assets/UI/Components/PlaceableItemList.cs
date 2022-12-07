using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class PlaceableItemList : MonoBehaviour {
        VerticalLayoutGroup verticalLayoutGroup;
        void Start() {
            GameObject rowItemTemplate = gameObject.transform.GetChild(0).gameObject;

            rowItemTemplate.SetActive(false);
            
            foreach (var item in ARGameSession.current.Items) {
                GameObject rowItem = GameObject.Instantiate(
                    rowItemTemplate, 
                    Vector3.zero, 
                    Quaternion.identity, 
                    gameObject.transform
                );
                rowItem.SetActive(true);
                PlaceableItem placeableItem = rowItem.AddComponent<PlaceableItem>();
                placeableItem.Label = item.Label;
                // placeableItem.PlaceableObject = item.PlaceableObject;

                // HACK: When the root CanvasScaler is in "Screen Space - Camera" mode,
                // Unity sets the position and rotation to weird values after instantiation
                rowItem.transform.localPosition = Vector3.zero;
                rowItem.transform.localRotation = Quaternion.identity;
            }
        }
    }
}