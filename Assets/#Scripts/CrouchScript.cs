using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchScript : MonoBehaviour
{
    public CharacterController Player;
    public bool IsCrouched;

    void Start()
    {

    }


    void Update()
    {
        CrouchBehavior();
    }


    public void CrouchBehavior()
    {
        if (Input.GetKey(KeyCode.C))
        {
            Player.height = 1.5f;
        }
        else
        {
            Player.height = 3;

        }
    }
}
