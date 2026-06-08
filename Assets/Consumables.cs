using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumables : MonoBehaviour
{
    public GameObject Pills;
    public Transform PillPos;

    public Animator ArmAnim;
    public GameObject LeftHand;
    void Start()
    {
        ArmAnim = gameObject.GetComponent<Animator>();
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ArmAnim.Play("Drink");
        }
    }

    public void PillStart()
    {
        var myNewPills = Instantiate(Pills,PillPos.position,PillPos.rotation);
        myNewPills.transform.parent = LeftHand.transform;
        myNewPills.transform.position = PillPos.position;
        myNewPills.transform.rotation = PillPos.rotation;
    }

}
