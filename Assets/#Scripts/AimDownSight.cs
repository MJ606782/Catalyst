using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class AimDownSight : MonoBehaviour
{
    public GameObject Crosshair;
    public Vector3 DownView;
    public Vector3 AimView;
    public Vector3 HipView;
    public GameObject Player;
    public GameObject Scope;

    public float ScopeView;
    
    public GameObject Camera;
    void Start()
    {
        
    }


    void Update()
    {
        if (Player.GetComponent<FirstPersonController>().m_IsWalking == true)
        {
            if (Input.GetButton("Fire2"))
            {
                gameObject.GetComponent<WeaponSway>().enabled = false;
                gameObject.GetComponent<Animator>().Play("Idle");
                gameObject.GetComponent<Animator>().enabled = false;
                transform.localPosition = Vector3.Slerp(transform.localPosition, AimView, 15 * Time.deltaTime);


                if(gameObject.tag == "Sniper")
                {

                    Scope.SetActive(true);
                    Camera.GetComponent<Camera>().fieldOfView = ScopeView;
                }
            }
        }
        if (Player.GetComponent<FirstPersonController>().m_IsWalking == true)
        {
            if (Input.GetButtonDown("Sprint"))
            {
                gameObject.GetComponent<Animator>().enabled = true;
                gameObject.GetComponent<Animator>().Play("Idle");               

            }


            if (Input.GetButtonUp("Fire2"))
            {              
                gameObject.GetComponent<Animator>().enabled = true;
                gameObject.GetComponent<Animator>().Play("Idle");
                StartCoroutine("DisableAnimator");
                gameObject.GetComponent<WeaponSway>().enabled = true;

                if (gameObject.tag == "Sniper")
                {
                    Scope.SetActive(false);
                    Camera.GetComponent<Camera>().fieldOfView = 70;

                }

            }
        }
    }
    private IEnumerator DisableAnimator()
    {
        yield return new WaitForSeconds(0.2f);
        gameObject.GetComponent<Animator>().enabled = false;
    }

    

}
