using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;

    //Camera follow speed
    public float followSpeed = 3;

    //Camera offset position
    public Vector3 followOffset;

    //Camera top down perspective when player is hidden
    public Vector3 topDownOffset;
    public float lookSpeed = 5;
 
    void LateUpdate()
    {
        //If player is hidden change to topdown mode for a better view of the enemy
        //Bird's Eye View Assassin's Creed Reference 
       if(PlayerController.IsHidden)
            TopDownMode();
       else
            FollowCameraMode();
    }


    //Camera follows the player 
    void FollowCameraMode()
    {
        Vector3 newPositiion = target.transform.position - target.transform.forward + target.transform.TransformVector(followOffset);
        transform.position = Vector3.Lerp(transform.position, newPositiion, followSpeed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, target.transform.forward, lookSpeed *Time.deltaTime);
    }

    //Change the position of the camera to topdown
    void TopDownMode()
    {
        Vector3 newPositiion = target.transform.position + topDownOffset;
        transform.position = Vector3.Lerp(transform.position, newPositiion, followSpeed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, Vector3.down, lookSpeed * Time.deltaTime);
    }
}
