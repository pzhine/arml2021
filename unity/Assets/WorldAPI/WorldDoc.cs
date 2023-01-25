using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using MongoDB.Bson;

#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARKit;
#endif

namespace WorldAsSupport.WorldAPI {
    public class WorldDoc {
        private Dictionary<string, Anchor> m_Anchors; 
        public Dictionary<string, Anchor> Anchors {
            get {
                if (m_Anchors == null) {
                    m_Anchors = new Dictionary<string, Anchor>();
                    // deserialize the anchors
                    if (CurrentVersion != null) {
                        DeserializeAnchors(CurrentVersion.Data.anchors);
                    }
                }
                return m_Anchors;
            }
            set {
                m_Anchors = value;
            }
        }

        public string DocDir {
            get {
                string path = Path.Combine(
                    WorldDatabase.current.DataDir,
                    Data._id
                );
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public string VersionDir {
            get {
                string path = Path.Combine(DocDir, "versions/" + Data.currentVersion);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public WorldDocData Data;

        private byte[] m_WorldMapBytes;
        public byte[] WorldMapBytes {
            get {
                if (m_WorldMapBytes == null) {
                    m_WorldMapBytes = ReadMapFile();
                }
                return m_WorldMapBytes;
            }
        }
    
    #if UNITY_IOS && !UNITY_EDITOR
        private NativeArray<byte> m_NativeMapBytes;

        private ARWorldMap m_WorldMap;
        public ARWorldMap WorldMap {
            get {
                if (!m_WorldMap.valid && WorldMapBytes != null) {
                    // read, deserialize and load WorldMap 
                    Debug.Log("[WorldDoc] Deserialize WorldMap");
                    m_NativeMapBytes = new NativeArray<byte>(
                        WorldMapBytes.Length, 
                        Allocator.Temp
                    );
                    
                    m_NativeMapBytes.CopyFrom(WorldMapBytes);
                    if (!ARWorldMap.TryDeserialize(m_NativeMapBytes, out m_WorldMap)) {
                        throw new Exception("ARWorldMap.TryDeserialize failed");
                    }
                }
                return m_WorldMap;
            }
        }
    #endif

        private FakeARWorldMapData m_FakeWorldMap; // for editor
        public FakeARWorldMapData FakeWorldMap {
            get {
                if (m_FakeWorldMap == null && File.Exists(MapDataPath)) {
                    // read, deserialize and load FakeWorldMap
                    using (StreamReader mapReader = new StreamReader(MapDataPath)) {
                        string mapJson = mapReader.ReadToEnd();
                        m_FakeWorldMap = JsonUtility.FromJson<FakeARWorldMapData>(mapJson);
                    }
                }
                return m_FakeWorldMap;
            }
        }

        private WorldVersion m_CurrentVersion;
        public WorldVersion CurrentVersion {
            get {
                Debug.Log("[WorldDoc.CurrentVersion.get] VersionDataPath: " + VersionDataPath);
                if (m_CurrentVersion == null && File.Exists(VersionDataPath)) {
                    // read VersionJson
                    using (StreamReader versionReader = new StreamReader(VersionDataPath)) {
                        string versionJson = versionReader.ReadToEnd();
                        CurrentVersion = new WorldVersion(
                            JsonUtility.FromJson<WorldVersionData>(versionJson)
                        );
                    }
                }
                return m_CurrentVersion;
            }
            set {
                m_CurrentVersion = value;
                if (value != null) {
                    m_CurrentVersion.Data.worldDoc = Data._id;
                    Data.currentVersion = m_CurrentVersion.Data._id;    
                }
            }
        }

        private string DocDataPath {
            get {
                return Path.Combine(DocDir, "WorldDoc.json");
            }
        }

        private string PreviousDocDataPath {
            get {
                return Path.Combine(DocDir, "PreviousWorldDoc.json");
            }
        }

        private string  VersionDataPath {
            get {
                return Path.Combine(VersionDir, "WorldVersion.json");
            }
        }

        private string MapDataPath {
            get {
                return Path.Combine(VersionDir, "Map.bin");
            }
        }

        public WorldDoc() {
            // init Data for new document
            Data = new WorldDocData();
            Data._id = ObjectId.GenerateNewId().ToString();
        }

        public WorldDoc(string id) {
            // init _id for existing document
            Data = new WorldDocData();
            Data._id = id;

            // load the Data for the given id
            if (!LoadData()) {
                Data = null;
            }
        }

        public WorldDoc(
            WorldDocData data,
            WorldVersionData versionData,
            byte[] worldMapBytes,
            FakeARWorldMapData fakeARWorldMapData
        ) {
            Data = data;
            WorldVersion version = new WorldVersion(versionData);
            m_CurrentVersion = version;
            m_WorldMapBytes = worldMapBytes;
            m_FakeWorldMap = fakeARWorldMapData;
        }

    #if UNITY_IOS && !UNITY_EDITOR
        public async Task<ARWorldMap> GetWorldMapAsync() {
            ARKitSessionSubsystem subsystem = 
                (ARKitSessionSubsystem)ARGameSession.current.ARSession.subsystem;

            var request = subsystem.GetARWorldMapAsync();
            
            while (!request.status.IsDone()) {
                await Task.Delay(50);
            }

            if (request.status.IsError())
            {
                throw new Exception(string.Format("Session serialization failed with status {0}", request.status));
            }

            return request.GetWorldMap();
        }
    #endif

        public byte[] ReadMapFile() {
            if (!File.Exists(MapDataPath)) {
                Debug.Log("[ReadMapFile] Map file not found");
                return null;
            }
            List<byte> allBytes = new List<byte>();
            using (FileStream file = File.Open(MapDataPath, FileMode.Open)) {
                int bytesPerFrame = 1024 * 10;
                long bytesRemaining = file.Length;
                BinaryReader binaryReader = new BinaryReader(file);
                while (bytesRemaining > 0) {
                    var bytes = binaryReader.ReadBytes(bytesPerFrame);
                    allBytes.AddRange(bytes);
                    bytesRemaining -= bytesPerFrame;
                }
            }
            return allBytes.ToArray();
        }

        private WorldDocAnchorItemData SerializeAnchorItem(AnchorItemRef itemRef) {
            WorldDocAnchorItemData itemData = new WorldDocAnchorItemData();
            if (itemRef.ItemInstance == null) {
                return null;
            }
            itemData.itemId = itemRef.ItemInstance.Label;
            itemData.instanceId = itemRef.ItemInstance.name;
            itemData.movedFromAnchorId = itemRef.MovedFromAnchorId;
            itemData.position = itemRef.ItemInstance.transform.position;
            itemData.rotation = itemRef.ItemInstance.transform.rotation;
            itemData.scale = itemRef.ItemInstance.transform.localScale;
            return itemData;
        }

        public WorldDocAnchorData[] SerializeAnchors() {
            List<WorldDocAnchorData> anchorList = new List<WorldDocAnchorData>();
            
            foreach (Anchor anchor in Anchors.Values) {
                WorldDocAnchorData anchorData = new WorldDocAnchorData();
                anchorData.id = anchor.NativeId;

                // serialize item
                anchorData.item = SerializeAnchorItem(anchor.Item);
                if (anchorData.item == null) {
                    continue;
                }

                // serialize child items, if there are any
                List<WorldDocAnchorItemData> itemDataList = new List<WorldDocAnchorItemData>();
                foreach (AnchorItemRef itemRef in anchor.ChildItems) {
                    itemDataList.Add(SerializeAnchorItem(itemRef));
                }
                anchorData.childItems = itemDataList.ToArray();

                anchorList.Add(anchorData);
            }
            return anchorList.ToArray();
        }

        public void DeserializeAnchors(WorldDocAnchorData[] anchorArray) {
            foreach (WorldDocAnchorData anchorData in anchorArray) {
                Anchor anchor = new Anchor();
                anchor.NativeId = anchorData.id;

                // deserialize item
                anchor.Item = new AnchorItemRef();
                anchor.Item.data = anchorData.item;
                anchor.Item.MovedFromAnchorId = anchorData.item.movedFromAnchorId;

                // deserialize child items
                foreach (WorldDocAnchorItemData anchorItemData in anchorData.childItems) {
                    AnchorItemRef itemRef = new AnchorItemRef();
                    itemRef.data = anchorItemData;
                    anchor.ChildItems.Add(itemRef);
                }
                
                m_Anchors[anchorData.id] = anchor;
            }
        }

        public bool LoadData() {
            try {
                // read DocJson
                using (StreamReader docReader = new StreamReader(DocDataPath)) {
                    string docJson = docReader.ReadToEnd();
                    Data = JsonUtility.FromJson<WorldDocData>(docJson);
                }
                return true;
            } catch (Exception ex) {
                Debug.Log("[LoadData] Error loading file: " + ex.ToString());
                return false;
            }
        }

        public async Task SaveData() {
            // create directories if needed
            if (!Directory.Exists(VersionDir)) {
                Directory.CreateDirectory(VersionDir);
            }
            // save current doc as "PreviousWorldDoc"
            if (File.Exists(DocDataPath)) {
                // first remove old "PreviousWorldDoc" to be safe
                File.Delete(PreviousDocDataPath);
                File.Move(DocDataPath, PreviousDocDataPath);
            }
            // save CurrentVersion as PreviousVersion
            WorldVersion PreviousVersion = CurrentVersion;

            try {
                // create a new version
                WorldVersion nextVersion = new WorldVersion();

                // serialize anchors dict to doc data
                nextVersion.Data.anchors = SerializeAnchors();

                // commit CurrentVersion
                CurrentVersion = nextVersion;

                // serialize the doc data to JSON
                string docJson = JsonUtility.ToJson(Data);
                string versionJson = JsonUtility.ToJson(nextVersion.Data);

                // write DocJson
                using (StreamWriter docWriter = new StreamWriter(DocDataPath)) {
                    Debug.Log("Writing WorldDoc to: " + DocDataPath);
                    docWriter.Write(docJson);
                }
                using (StreamWriter versionWriter = new StreamWriter(VersionDataPath)) {
                    Debug.Log("Writing WorldVersion to: " + VersionDataPath);
                    versionWriter.Write(versionJson);
                }

    #if !UNITY_EDITOR && UNITY_IOS
                // save serialized WorldMap
                ARWorldMap worldMap = await GetWorldMapAsync();
                using (FileStream mapStream = File.Open(MapDataPath, FileMode.Create)) {
                    NativeArray<byte> data = worldMap.Serialize(Allocator.Temp);
                    BinaryWriter mapWriter = new BinaryWriter(mapStream);
                    mapWriter.Write(data.ToArray());
                    mapWriter.Close();
                    data.Dispose();
                    worldMap.Dispose();
                }
    #else
                // save serialized "fake anchors"
                string mapJson = JsonUtility.ToJson(GetFakeARWorldMap());
                using (StreamWriter mapWriter = new StreamWriter(MapDataPath)) {
                    Debug.Log("Writing WorldMap to: " + MapDataPath);
                    mapWriter.Write(mapJson);
                }
    #endif
            } catch (Exception ex) {
                Debug.Log("EXCEPTION " + ex);
                // rollback to previous version
                if (File.Exists(PreviousDocDataPath)) {
                    File.Copy(PreviousDocDataPath, DocDataPath, true);
                }
                CurrentVersion = PreviousVersion;
                throw ex;
            }
        }

        private FakeARWorldMapData GetFakeARWorldMap() {
            FakeARWorldMapData worldMap = new FakeARWorldMapData();
            FakeARAnchorData[] anchorArray = new FakeARAnchorData[Anchors.Count];

            int i = 0;
            foreach(Anchor anchor in Anchors.Values) {
                FakeARAnchorData anchorData = new FakeARAnchorData();
                anchorData.id = anchor.NativeId;
                anchorData.position = anchor.Item.ItemInstance.transform.parent.position;
                anchorData.rotation = anchor.Item.ItemInstance.transform.parent.rotation;
                anchorArray[i] = anchorData;
                i++;
            }
            
            worldMap.anchors = anchorArray;
            return worldMap;
        }
    }
}

