using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldAsSupport;
using WorldAsSupport.Research;
using UnityStandardAssets.Characters.ThirdPerson;

public class ExperiencesManager : MonoBehaviour
{
    private GameObject player;
    private GameObject initialPosition;

    public GameObject IntroModal_WOW;
    public bool singlePlayer;
    public bool isWindow_on_the_World;

    [SerializeField] private GameObject SecondPlayer;
    [SerializeField] private Camera WoWCamera;
    [SerializeField] private GameObject WoWPlayer;

    // Start is called before the first frame update
    void Start()
    {
        player = ARGameSession.current.ARCamera.transform.parent.gameObject;

        if(isWindow_on_the_World){
            ARGameSession.current.Player.SetActive(false);
            WoWPlayer.SetActive(true);
            ARGameSession.current.HUDCanvas.GetComponent<Canvas>().worldCamera = WoWCamera;
            ARGameSession.current.PlayerViewCamera = WoWCamera;
            ARGameSession.current.ModalsCanvas.GetComponent<Canvas>().worldCamera = WoWCamera;
            GameObject.Find("TargetCanvas").GetComponent<Canvas>().worldCamera = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
            player = WoWPlayer;
            ExperimentManager.current.IntroModalPrefab = IntroModal_WOW;
        }

        if(!singlePlayer){
            SecondPlayer.SetActive(true);
            ARGameSession.current.PlayerViewCamera = SecondPlayer.GetComponentInChildren<Camera>();
            ARGameSession.current.ARCamera.GetComponent<EditorCameraControl>().enabled = false;
            ARGameSession.current.Player.GetComponent<ThirdPersonCharacter>().enabled = false;
            ARGameSession.current.Player.GetComponent<ThirdPersonUserControl>().enabled = false;

            WoWCamera.GetComponent<EditorCameraControl>().enabled = false;
            WoWCamera.GetComponent<Camera>().enabled = false;
            WoWPlayer.GetComponent<ThirdPersonCharacter>().enabled = false;
            WoWPlayer.GetComponent<ThirdPersonUserControl>().enabled = false;
            
            ARGameSession.current.HUDCanvas.GetComponent<Canvas>().worldCamera = SecondPlayer.GetComponentInChildren<Camera>();
            ARGameSession.current.ModalsCanvas.GetComponent<Canvas>().worldCamera = SecondPlayer.GetComponentInChildren<Camera>();
            GameObject.Find("TargetCanvas").GetComponent<Canvas>().worldCamera = SecondPlayer.GetComponentInChildren<Camera>();
        }

        initialPosition = GameObject.Find("InitialPosition");
        if (initialPosition == null) {
            return;
        }
        player.transform.position = initialPosition.transform.position;
        player.transform.rotation = initialPosition.transform.rotation;
    }
}
