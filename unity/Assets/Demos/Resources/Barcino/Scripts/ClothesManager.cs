using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothesManager : MonoBehaviour
{
    public GameObject[] towels; //different towel states

    void Start()
    {
        startFirst();
    }

    //enable - disable towel states
    void changeActivation(bool a, bool b, bool c)
    {
        towels[0].SetActive(a);
        towels[1].SetActive(b);
        towels[2].SetActive(c);
    }

    private void startFirst()
    {
        //load the towel from the prefabs and set its position to the parent's position
        var a = Instantiate(towels[0], new Vector3(0, 0, 0), Quaternion.identity);
        a.transform.parent = gameObject.transform;
        a.transform.position = gameObject.transform.position;

        towels[0] = a;
        changeActivation(true, false, false); //just activate the first towel
    }

    public void firstToSecond()
    {
        var a = Instantiate(towels[1], new Vector3(0, 0, 0), Quaternion.identity);
        a.transform.parent = gameObject.transform;
        
        towels[1] = a;
        //Destroy(a);
        towels[1].transform.position = towels[0].transform.position;
        this.GetComponent<BoxCollider>().enabled = false;
        changeActivation(false, true, false);
    }

    public void secondToThird(GameObject hanger)
    {
        var a = Instantiate(towels[2], new Vector3(0, 0, 0), Quaternion.identity);
        a.transform.parent = gameObject.transform;
        towels[2] = a;
 
        // get capsule collider from hanger
        CapsuleCollider[] cc = new CapsuleCollider[1];
        cc[0] = hanger.GetComponent<CapsuleCollider>();

        // disable box collider from hanger (this is where the ray interacts)
        this.gameObject.GetComponent<BoxCollider>().enabled = false;
        
        // activate third towel state
        towels[2].GetComponent<Cloth>().capsuleColliders = cc;
        towels[2].transform.rotation = hanger.GetComponent<CapsuleCollider>().transform.rotation;
        towels[2].transform.Rotate(90, 0, 0);
        towels[2].transform.position = hanger.transform.position + new Vector3(0,0.01f,0);
        changeActivation(false, false, true);
    }


}
