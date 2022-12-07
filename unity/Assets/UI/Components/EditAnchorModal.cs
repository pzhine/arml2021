using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class EditAnchorModal : ModalWindow {
        private InputField m_NameInput; 

        public void OnDeleteButtonPressed() {
            ARGameSession.current.AnchorProvider.RemoveAnchor(
                ARGameSession.current.ItemToEdit.Anchor.NativeId
            );
            ARGameSession.current.ItemToEdit = null;
            Dismiss();
        }

        public void OnFindNewAnchorButtonPressed() {
            ARGameSession.current.ItemToPlace = ARGameSession.current.ItemToEdit;
            Dismiss();
        }
    }
}