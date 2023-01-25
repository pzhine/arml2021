using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WorldAsSupport {
    namespace WorldAPI {
        class WorldDatabase : MonoBehaviour {
            // singleton instance
            public static WorldDatabase current;

            // Database root path
            private string m_DataDir;
            public string DataDir {
                get {
                    if (m_DataDir == null) {
                        throw new Exception("Trying to access DataDir before Awake()");
                    }
                    if (!Directory.Exists(m_DataDir)) {
                        Directory.CreateDirectory(m_DataDir);
                    }
                    return m_DataDir;
                }
            }

            public List<WorldDoc> GetLocalDocList() {
                List<WorldDoc> WorldDocs = new List<WorldDoc>();
                foreach (string docId in Directory.EnumerateDirectories(DataDir)) {
                    WorldDoc worldDoc = new WorldDoc(docId);
                    if (worldDoc.Data != null) {
                        WorldDocs.Add(worldDoc);
                    }
                }
                return WorldDocs;
            }

            public void Awake() {
                current = this;
                current.m_DataDir = Path.Combine(
                    Application.persistentDataPath, 
                    "WorldDatabase"
                );
            }
        }
    }
}
