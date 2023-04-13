using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using MongoDB.Bson;
using WorldAsSupport.WorldAPI;

/// <summary>
/// Provides methods for adding anchors and persisting them in the WorldDoc
/// </summary>
namespace WorldAsSupport
{
    public class AnchorItemRef
    {
        public WorldDocAnchorItemData data;

        // PlaceableItem associated with this anchor
        private PlaceableItem m_ItemInstance;
        public PlaceableItem ItemInstance
        {
            get
            {
                return m_ItemInstance;
            }
            set
            {
                m_ItemInstance = value;

                // If WorldDocAnchorItemData exists on the item, we're doing a restore,
                //   so restore the position and rotation
                if (value != null && data != null)
                {
                    m_ItemInstance.transform.position = data.position;
                    m_ItemInstance.transform.rotation = data.rotation;
                    m_ItemInstance.transform.localScale = data.scale;
                }
            }
        }

        // If the item was moved from another anchor, this will record the move
        private string m_MovedFromAnchorId;
        public string MovedFromAnchorId
        {
            get
            {
                return m_MovedFromAnchorId;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_MovedFromAnchorId = null;
                }
                else
                {
                    m_MovedFromAnchorId = value;
                }
            }
        }
    }

    /// <summary>
    // Anchor represents the ARAnchor and associated PlaceableItem
    /// </summary>
    public class Anchor
    {
        // ARKit Anchor Id
        public string NativeId;

        // Native ARAnchor
        public ARAnchor NativeAnchor;

        // Primary PlaceableItem
        public AnchorItemRef Item;

        // Child PlaceableItems
        public List<AnchorItemRef> ChildItems;

        // ransform
        public Transform Transform
        {
            get => NativeAnchor?.transform ?? m_Transform;
            set
            {
                Debug.Log("[Anchor] set transform: " + value);
                m_Transform = value;
            }
        }

        public Anchor()
        {
            ChildItems = new List<AnchorItemRef>();
        }

        private Transform m_Transform;
    }

    public class FakeARAnchor
    {
        public string id;
        public Transform transform;

        public FakeARAnchor(string id)
        {
            this.id = id;
        }
    }

    public class AnchorProvider
    {
        private ARAnchorManager m_ARAnchorManager;
        public bool WaitingForRestore = false;

        public AnchorProvider(ARAnchorManager arAnchorManager)
        {
            m_ARAnchorManager = arAnchorManager;
            m_ARAnchorManager.anchorsChanged += m_OnAnchorsChanged;
        }

        public void OnDestroy()
        {
            m_ARAnchorManager.anchorsChanged -= m_OnAnchorsChanged;
        }

        public Anchor PlaceAnchor(Pose pose, PlaceableItem item)
        {
            Debug.Log("PlaceAnchor: " + item.Label);
            Anchor anchor = new Anchor();
            PlaceableItem itemInstance;
#if UNITY_IOS && !UNITY_EDITOR
            ARAnchor arAnchor = m_ARAnchorManager.AddAnchor(pose);
            anchor.NativeId = AnchorProvider.GetAnchorKey(arAnchor);
            anchor.NativeAnchor = arAnchor;
            itemInstance = 
                ARGameSession.current.InstantiatePlaceableItem(item, anchor, arAnchor.transform);
#else
            // in the simulator, generate a fake anchor
            anchor.NativeId = ObjectId.GenerateNewId().ToString();
            Transform anchorTransform = PlaceFakeAnchor(pose);
            itemInstance =
                ARGameSession.current.InstantiatePlaceableItem(item, anchor, anchorTransform);
#endif
            // rotate the item to look at ARCamera
            itemInstance.LookAtCamera();
            AddItemToAnchor(anchor, itemInstance);

            //persist anchor ID to WorldDoc
            ARGameSession.current.WorldDoc.Anchors[anchor.NativeId] = anchor;

            // make the item instance the ItemToEdit
            ARGameSession.current.ItemToPlace = null;
            ARGameSession.current.ItemToEdit = itemInstance;

            return anchor;
        }

        // Create an AnchorItemRef for the PlaceableItem instance and add it to the anchor
        public void AddItemToAnchor(Anchor anchor, PlaceableItem itemInstance)
        {
            // create an AnchorItemRef for the item
            anchor.Item = new AnchorItemRef();

            // attach item to ItemRef
            anchor.Item.ItemInstance = itemInstance;

            // If instance has an existing Anchor assigned, it was moved,
            //   so record the move
            // Only record the first move (don't update MovedFromAnchor if it's already set)
            //   to preserve the original Anchor
            if (itemInstance.Anchor != null && anchor.Item.MovedFromAnchorId == null)
            {
                anchor.Item.MovedFromAnchorId = itemInstance.Anchor.NativeId;
            }
            // Update the Anchor on the item to the current anchor
            itemInstance.Anchor = anchor;

            // Iterate over child PlaceableItems (if they exist) and create child ItemRefs for them
            // Throw an error if there are duplicate names, because that will break the save/restore
            Dictionary<string, bool> namesDict = new Dictionary<string, bool>();
            PlaceableItem[] childItems = itemInstance.GetComponentsInChildren<PlaceableItem>();
            foreach (PlaceableItem childItem in childItems)
            {
                // skip this item if it is the primary item
                if (childItem == itemInstance)
                {
                    continue;
                }

                // check for duplicates
                if (namesDict.ContainsKey(childItem.name))
                {
                    throw new System.Exception("Child PlaceableItems must have unique names. Duplicate found for: " + childItem.name);
                }
                namesDict[childItem.name] = true;

                // create an AnchorItemRef and attach it to the child
                AnchorItemRef itemRef = new AnchorItemRef();
                itemRef.ItemInstance = childItem;
                childItem.Anchor = anchor;

                // add the ItemRef to ChildItems
                anchor.ChildItems.Add(itemRef);
            }
        }

        public void RemoveAnchor(string nativeId)
        {
            Dictionary<string, Anchor> Anchors = ARGameSession.current.WorldDoc.Anchors;
            Anchor anchor = Anchors[nativeId];

#if UNITY_IOS && !UNITY_EDITOR
            m_ARAnchorManager.RemoveAnchor(anchor.NativeAnchor);
#else
            // destroy the GameObjects of the anchor's item
            ARGameSession.current.DestroyGameObject(
                anchor.Item.ItemInstance.transform.parent.gameObject
            );
#endif

            // remove the anchor from the doc
            Anchors.Remove(nativeId);
        }

        public Anchor GetAnchor(string nativeId)
        {
            Dictionary<string, Anchor> Anchors = ARGameSession.current.WorldDoc.Anchors;
            Anchor anchor = Anchors[nativeId];
            return anchor;
        }

        public static string GetAnchorKey(ARAnchor anchor)
        {
            if (anchor == null)
            {
                return "FAKE_ANCHOR";
            }
            return anchor.trackableId.ToString();
        }

        public void RestoreAllAnchors()
        {
            foreach (string anchorKey in ARGameSession.current.WorldDoc.Anchors.Keys)
            {
                Anchor anchor = ARGameSession.current.WorldDoc.Anchors[anchorKey];
                Debug.Log("[AnchorProvider.RestoreAllAnchors] " + anchor?.NativeId);
                Debug.Log("[AnchorProvider.RestoreAllAnchors] " + anchor?.Transform);
                RestoreAnchor(anchorKey, anchor.Transform, anchor.NativeAnchor);
            }
        }

        // Instantiate PlaceableItems when anchors are created during loading of a WorldMap.
        private void RestoreAnchor(string anchorKey, Transform anchorTransform, ARAnchor arAnchor)
        {
            if (!ARGameSession.current.WorldDoc.Anchors.ContainsKey(anchorKey))
            {
                // an ARAnchor was saved (or left) for a item that no longer exists
                Debug.Log("[AnchorProvider.RestoreAnchor] WARNING anchor no longer exists for key: " + anchorKey);
                return;
            }
            Anchor anchor = ARGameSession.current.WorldDoc.Anchors[anchorKey];

            // Assign the transform. If this is not a fake anchor, this has no effect
            // because the transform will come from the native anchor
            anchor.Transform = anchorTransform;

            // If anchor already has an item instance assigned
            //   it's already been restored or placed, so bail early
            if (anchor.Item.ItemInstance)
            {
                return;
            }

            Debug.Log("[AnchorProvider.RestoreAnchor] " + anchor.Item.data.itemId);

            PlaceableItem item;

            // If the item was moved from a different anchor, 
            //   try to find the instance in the MovedFromAnchor
            // If that anchor hasn't been instantiated yet, bail early and we'll hit it in pass 2
            if (anchor.Item.MovedFromAnchorId != null)
            {
                Debug.Log("[AnchorProvider.RestoreAnchor] MovedFromAnchorId " + anchor.Item.MovedFromAnchorId);
                Anchor fromAnchor = ARGameSession.current.WorldDoc.Anchors[anchor.Item.MovedFromAnchorId];
                if (fromAnchor.Item.ItemInstance == null)
                {
                    return;
                }
                List<PlaceableItem> fromInstances
                    = new List<PlaceableItem>(fromAnchor.Item.ItemInstance.GetComponentsInChildren<PlaceableItem>());
                item = fromInstances.Find(child => child.name == anchor.Item.data.instanceId);
            }
            else
            {
                // otherwise, get the item from inventory
                item = ARGameSession.current.ItemsDict[anchor.Item.data.itemId];
            }

            // Instantiate the primary PlaceableItem
            anchor.Item.ItemInstance =
                ARGameSession.current.InstantiatePlaceableItem(item, anchor, anchorTransform);

            // attach the anchor and native anchor
            anchor.Item.ItemInstance.Anchor = anchor;
            anchor.NativeAnchor = arAnchor;
            anchor.NativeId = AnchorProvider.GetAnchorKey(arAnchor);

            // Iterate over ChildItems, if they exist
            //   find their corresponding PlaceableItem instance inside the primary PlaceableItem
            //   and attach that instance to the child ItemRef
            List<PlaceableItem> childInstances =
                new List<PlaceableItem>(anchor.Item.ItemInstance.GetComponentsInChildren<PlaceableItem>());
            foreach (AnchorItemRef itemRef in anchor.ChildItems)
            {
                Debug.Log("RestoreAnchor child instance: " + itemRef.data.itemId);

                // find the instance
                PlaceableItem itemInstance = childInstances.Find(
                    child => child.Anchor == null && child.Label == itemRef.data.itemId
                );

                // associate instance with itemRef
                itemRef.ItemInstance = itemInstance;
                itemInstance.Anchor = anchor;
            }
        }

        // This function is called by the ARKit subsystem when a new ARAnchor has been added to the world.
        private void m_OnAnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
        {
            if (!m_ARAnchorManager.subsystem.running)
            {
                return;
            }
            Debug.Log("OnAnchorsChanged");
            // We have to run through the restore in 2 passes, because some of the anchors depend on others
            //   already restored (e.g. for MovedFromAnchorId)
            for (int i = 0; i < 2; i++)
            {
                foreach (ARAnchor arAnchor in eventArgs.added)
                {
                    string anchorKey = GetAnchorKey(arAnchor);
                    RestoreAnchor(anchorKey, arAnchor.transform, arAnchor);
                }
            }
            if (eventArgs.added != null && eventArgs.added.Count > 0 && WaitingForRestore)
            {
                DisplayProvider.current.SetSecondaryDisplayActive(true);
                WaitingForRestore = false;
            }
        }

        public void ApplyFakeWorldMap(FakeARWorldMapData fakeWorldMap)
        {
            List<FakeARAnchor> fakeAnchors = new List<FakeARAnchor>();
            foreach (FakeARAnchorData fakeAnchorData in fakeWorldMap.anchors)
            {
                FakeARAnchor fakeAnchor = new FakeARAnchor(fakeAnchorData.id);
                Pose pose = new Pose(fakeAnchorData.position, fakeAnchorData.rotation);
                fakeAnchor.transform = PlaceFakeAnchor(pose);
                fakeAnchors.Add(fakeAnchor);
            }
            for (int i = 0; i < 2; i++)
            {
                Debug.Log("Restore pass: " + (i + 1));
                foreach (FakeARAnchor fakeAnchor in fakeAnchors)
                {
                    RestoreAnchor(fakeAnchor.id, fakeAnchor.transform, null);
                }
            }
        }

        private Transform PlaceFakeAnchor(Pose pose)
        {
            GameObject spawnedAnchor = ARGameSession.Instantiate(
                m_ARAnchorManager.anchorPrefab,
                pose.position,
                pose.rotation,
                ARGameSession.current.transform
            );
            return spawnedAnchor.transform;
        }

    }

}