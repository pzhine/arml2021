using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class ListWorldDocsModal : ModalWindow {
        private GameObject RowItemTemplate;
        public override void Awake() {
            base.Awake();
            RowItemTemplate = Content.GetChild(0).gameObject;
            RowItemTemplate.SetActive(false);
        }

        public override void Show() {
            ClearContent();
            List<WorldDoc> worldDocs = WorldDatabase.current.GetLocalDocList();
            foreach (WorldDoc worldDoc in worldDocs) {
                GameObject rowItem = GameObject.Instantiate(RowItemTemplate, Vector3.zero, Quaternion.identity, Content);
                rowItem.SetActive(true);
                WorldDocListItem worldDocListItem = rowItem.AddComponent<WorldDocListItem>();
                worldDocListItem.WorldDoc = worldDoc;
                // HACK: When the root CanvasScaler is in "Screen Space - Camera" mode,
                // Unity sets the position and rotation to weird values after instantiation
                rowItem.transform.localPosition = Vector3.zero;
                rowItem.transform.localRotation = Quaternion.identity;
            }

            base.Show();
        }
    }

    public class WorldDocListItem : MonoBehaviour {
        private WorldDoc m_WorldDoc;
        public WorldDoc WorldDoc {
            get {
                return m_WorldDoc;
            }
            set {
                m_WorldDoc = value;

                // set label text 
                Text ListRowLabel = GetComponentInChildren<Text>();
                ListRowLabel.text = WorldDoc.Data.name;
            }
        }

        public void OnItemPressed() {
            RemoteProvider rp = RemoteProvider.current;
            if (rp.Role == RemoteProviderRole.Sender &&
                rp.Status == RemoteProviderStatus.Connected
            ) {
                // dispatch the command
            #if !UNITY_EDITOR && UNITY_IOS
                rp.CommandDispatcher.LoadWorldDoc(
                    m_WorldDoc.Data,
                    m_WorldDoc.CurrentVersion.Data,
                    m_WorldDoc.WorldMapBytes
                );
            #else 
                rp.CommandDispatcher.LoadWorldDoc(
                    m_WorldDoc.Data,
                    m_WorldDoc.CurrentVersion.Data,
                    null,
                    m_WorldDoc.FakeWorldMap
                );
            #endif

            } else {
                // ARGameSession.current.WorldDoc = m_WorldDoc;
                WorldSceneLoader.current.LoadSceneWithWorldDoc(m_WorldDoc);
            }
            ARGameSession.current.DismissAllModals();
        }

        public void Awake() {
            // add button press handler
            Button itemButton = GetComponentInChildren<Button>();
            itemButton.onClick.AddListener(OnItemPressed);
        }
    }
}