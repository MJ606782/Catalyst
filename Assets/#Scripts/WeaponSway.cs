using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{


    public float Amount;
    public float MaxAmount;
    public float smoothAmount;

    private Vector3 initialPosition;

   
    void Start()
    {
        initialPosition = transform.localPosition;
    }


    void Update()
    {
        float movementX = -Input.GetAxis("Mouse X") * Amount;
        float movementY = -Input.GetAxis("Mouse Y") * Amount;

        movementX = Mathf.Clamp(movementX, -MaxAmount, MaxAmount);
        movementY = Mathf.Clamp(movementY, -MaxAmount, MaxAmount);
        Vector3 finalPosition = new Vector3(movementX, movementY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
}
