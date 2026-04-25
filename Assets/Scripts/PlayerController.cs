using UnityEngine;
using static Enemy;

public class PlayerController : MonoBehaviourInstance<PlayerController>
{
    public float speed = 5f;
    public float mouseSensitivity = 200f;

    public Transform cameraPivot;

    private CharacterController controller;
    private float xRotation = 0f;

    private void Start()
    {
        
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);
    }

    public void Hit(Weapon weapon)
    {
        //Determine damage
    }
}
