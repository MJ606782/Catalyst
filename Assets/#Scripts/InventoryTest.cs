using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTest : MonoBehaviour
{

    public GameObject Weapon1;
    public GameObject Weapon2;
    public GameObject Weapon3;
    public GameObject Weapon4;
    public GameObject Weapon5;

    public float ActiveWeapon = 1;

    void Update()
    {
        WeaponSwap();
        ActiveWeaponManager();
    }


    public void WeaponSwap()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ActiveWeapon = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ActiveWeapon = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActiveWeapon = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActiveWeapon = 3;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActiveWeapon = 4;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ActiveWeapon = 5;
        }
    }

    public void ActiveWeaponManager()
    {
        if (ActiveWeapon == 0)
        {
            Weapon1.SetActive(false);
            Weapon2.SetActive(false);
            Weapon3.SetActive(false);
            Weapon4.SetActive(false);
            Weapon5.SetActive(false);
        }

        if (ActiveWeapon == 1)
        {
            Weapon1.SetActive(true);
            Weapon2.SetActive(false);
            Weapon3.SetActive(false);
            Weapon4.SetActive(false);
            Weapon5.SetActive(false);
        }

        if (ActiveWeapon == 2)
        {
            Weapon1.SetActive(false);
            Weapon2.SetActive(true);
            Weapon3.SetActive(false);
            Weapon4.SetActive(false);
            Weapon5.SetActive(false);
        }

        if (ActiveWeapon == 3)
        {
            Weapon1.SetActive(false);
            Weapon2.SetActive(false);
            Weapon3.SetActive(true);
            Weapon4.SetActive(false);
            Weapon5.SetActive(false);
        }

        if (ActiveWeapon == 4)
        {
            Weapon1.SetActive(false);
            Weapon2.SetActive(false);
            Weapon3.SetActive(false);
            Weapon4.SetActive(true);
            Weapon5.SetActive(false);
        }

        if (ActiveWeapon == 5)
        {
            Weapon1.SetActive(false);
            Weapon2.SetActive(false);
            Weapon3.SetActive(false);
            Weapon4.SetActive(false);
            Weapon5.SetActive(true);
        }
    }
}
