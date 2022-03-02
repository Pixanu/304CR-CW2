using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;

    //camera follow speed
    public float followSpeed = 3;
    //camera offset position
    public Vector3 followOffset;
    //camera top down perspective when player is hidden
    public Vector3 topDownOffset;
    public float lookSpeed = 5;
 
    void LateUpdate()
    {
       if(PlayerController.IsHidden)
            TopDownMode();
       else
            FollowCameraMode();
    }


    void FollowCameraMode()
    {
        Vector3 newPositiion = target.transform.position - target.transform.forward + target.transform.TransformVector(followOffset);
        transform.position = Vector3.Lerp(transform.position, newPositiion, followSpeed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, target.transform.forward, lookSpeed *Time.deltaTime);
    }

    void TopDownMode()
    {
        Vector3 newPositiion = target.transform.position + topDownOffset;
        transform.position = Vector3.Lerp(transform.position, newPositiion, followSpeed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, Vector3.down, lookSpeed * Time.deltaTime);
    }
}
