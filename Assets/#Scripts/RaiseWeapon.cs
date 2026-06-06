using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseWeapon : MonoBehaviour
{
    public Transform GunPos;
    public Transform HipPos;
    public GameObject Crosshair;
    public GameObject Gun;

    void Start()
    {

    }


    void Update()
    {

        if (Input.GetButtonDown("Fire1"))
        {
            GunPos.position = HipPos.position;
            GunPos.rotation = HipPos.rotation;
            Gun.GetComponent<Gun>().enabled = true;
            Crosshair.SetActive(true);
            Gun.GetComponent<Animator>().enabled = true;
            Gun.GetComponent<RaiseWeapon>().enabled = false;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            GunPos.position = HipPos.position;
            GunPos.rotation = HipPos.rotation;
            Gun.GetComponent<Gun>().enabled = true;
            Crosshair.SetActive(true);
            Gun.GetComponent<Animator>().enabled = true;
            Gun.GetComponent<RaiseWeapon>().enabled = false;
        }
    }


}
