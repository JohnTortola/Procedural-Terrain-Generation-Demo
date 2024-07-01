using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 50f;
    public float rotationSpeed = 100f;
    public float speedVertical = 50f;

    void Update()
    {
        float moveInput = Input.GetAxis("Vertical");
        transform.Translate(Vector3.forward * moveInput * speed * Time.deltaTime);


        float rotateInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * speedVertical * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.F))
        {
            transform.Translate(Vector3.down * speedVertical * Time.deltaTime);
        }
    }
}
