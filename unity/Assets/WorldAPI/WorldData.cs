using UnityEngine;
using System;

namespace WorldAsSupport.WorldAPI {
    [Serializable]
    public class WorldDocData {
        public string _id;
        public string name;
        public string currentVersion;
    }

    [Serializable]
    public class WorldDocAnchorItemData {
        public string itemId;
        public string instanceId;
        public string movedFromAnchorId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    [Serializable]
    public class WorldDocAnchorData {
        public string id;
        public WorldDocAnchorItemData item;
        public WorldDocAnchorItemData[] childItems;
    }

    [Serializable]
    public class WorldVersionData {
        public string _id;
        public string worldDoc;
        public string lastModified;
        public string notes;
        public WorldDocAnchorData[] anchors;
    }

    [Serializable]
    public class FakeARAnchorData {
        public string id;
        public Vector3 position;
        public Quaternion rotation;
    } 

    [Serializable]
    public class FakeARWorldMapData {
        public FakeARAnchorData[] anchors;
    }
}