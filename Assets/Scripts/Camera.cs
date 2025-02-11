using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public float speed;

    void Update()
    {

        Cursor.lockState = CursorLockMode.Confined;

        Vector3 mousePos = Input.mousePosition;

        mousePos.x = mousePos.x / Screen.width;
        mousePos.y = mousePos.y / Screen.height;

        if(mousePos.x < 0.01f)
        {
            transform.Translate(Vector3.right * -speed * Time.deltaTime, Space.World);
        }

        if (mousePos.x > 0.993f)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        }

        if (mousePos.y > 0.982f)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
        }

        if (mousePos.y < 0.009f)
        {
            transform.Translate(Vector3.forward * -speed * Time.deltaTime, Space.World);
        }
    }
}
