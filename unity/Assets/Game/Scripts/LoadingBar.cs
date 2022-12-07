using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WorldAsSupport {
    public class LoadingBar : MonoBehaviour
    {
        public delegate void LoadingCompleteEvent();
        public event LoadingCompleteEvent LoadingComplete;
        
        private bool m_IsLoading;
        public bool IsLoading {
            get {
                return m_IsLoading;
            }
            set {
                m_IsLoading = value;
                this.GetComponent<Animator>().SetBool("isLoading", value);
            }
        }

        public static LoadingBar current;
        
        void Awake() {
            LoadingBar.current = this;
        }
        void loadCompleted()
        {
            Debug.Log("Load completed...");
            IsLoading = false;
            if (LoadingComplete != null) {
                LoadingComplete();
            }
        }
    }
}