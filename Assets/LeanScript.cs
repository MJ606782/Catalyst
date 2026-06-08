using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class LeanScript : MonoBehaviour
{
    public float MaxLeftLean;
    public float MaxRightLean;


    public FirstPersonController Player;
    void Start()
    {

    }

   
    void Update()
    {
        LeanBehavior();
    }


    public void LeanBehavior()
    {
        if(Player.m_IsWalking == true)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                var rotationVector = transform.rotation.eulerAngles;
                rotationVector.z = -35;
                transform.rotation = Quaternion.Euler(rotationVector);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                var rotationVector = transform.rotation.eulerAngles;
                rotationVector.z = 35;
                transform.rotation = Quaternion.Euler(rotationVector);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                var rotationVector = transform.rotation.eulerAngles;
                rotationVector.z = 0;
                transform.rotation = Quaternion.Euler(rotationVector);
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                var rotationVector = transform.rotation.eulerAngles;
                rotationVector.z = 0;
                transform.rotation = Quaternion.Euler(rotationVector);
            }
        }
    }
}
