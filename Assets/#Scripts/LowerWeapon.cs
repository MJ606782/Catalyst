using UnityEngine;

public class LowerWeapon : MonoBehaviour
{
    public GameObject Gun;
    public GameObject Crosshair;
    public Transform GunPos;
    public Transform LowerPos;
    public Transform HipPos;

    public bool IsRunning;

    void Update()
    {
        // FIXED: Changed from GetKeyDown to GetKey so it stays true while holding the button
        if (Input.GetKey(KeyCode.LeftShift))
        {
            IsRunning = true;
        }
        else
        {
            IsRunning = false;
        }

        LowerTheWeapon();
    }

    public void LowerTheWeapon()
    {
        if (IsRunning)
        {
            if (Gun.GetComponent<Animator>() != null) Gun.GetComponent<Animator>().enabled = false;
            Crosshair.SetActive(false);
            GunPos.position = LowerPos.position;
            GunPos.rotation = LowerPos.rotation;
        }
        else
        {
            if (Gun.GetComponent<Animator>() != null) Gun.GetComponent<Animator>().enabled = true;
            Crosshair.SetActive(true);
        }
    }
}