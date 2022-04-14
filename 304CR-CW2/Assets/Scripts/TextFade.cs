using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFade :  MonoBehaviour
{

    public GameObject uiObject;
    private void Start()
    {
        //obj is deactivated at start
        uiObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider player)
    {
        // If the player with its specific tag gets near the the object activate it and leave the text on the screen for "x" seconds
        if(player.gameObject.tag == "Player")
        {
            uiObject.SetActive(true);
            StartCoroutine("WaitForSec");
        }
    }

    IEnumerator WaitForSec()
    {
        yield return new WaitForSeconds(5);
        Destroy(uiObject);
        Destroy(gameObject);
    }





}