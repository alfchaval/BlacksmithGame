using System.Collections;
//using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourInstance<PlayerController>
{
    [Header("Settings")]
    public float speed = 5f;
    public float jumpHeight = 1;
    public float mouseSensitivity = 200f;
    public CharacterController controller;
    public Transform cameraPivot;

    [Header("UI")]
    public Image weaponImage;
    public Image shieldImage;
    public SpriteAnimation weaponChargeAnimation;
    public SpriteAnimation weaponHitAnimation;
    public SpriteAnimation shieldAnimation;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;
    private float ySpeed;
    private bool jumping;
    private bool chargingAttack;

    private Coroutine weaponCoroutine = null;
    private Coroutine shieldCoroutine = null;

    private void Start()
    {
        GameManager.GetInstance().playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        GameManager.GetInstance().playerInput.actions["Move"].canceled += ctx => moveInput = Vector2.zero;
        GameManager.GetInstance().playerInput.actions["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        GameManager.GetInstance().playerInput.actions["Look"].canceled += ctx => lookInput = Vector2.zero;
        GameManager.GetInstance().playerInput.actions["Attack"].performed += ctx => StartAttack();
        GameManager.GetInstance().playerInput.actions["Attack"].canceled += ctx => EndAttack();
    }

    private void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (controller.isGrounded)
        {
            if (ySpeed < 0)
                ySpeed = -2f;

            if (jumping)
            {
                ySpeed = Mathf.Sqrt(jumpHeight * -2f * Utils.gravity);
            }
        }
        else
        {
            ySpeed -= Utils.gravity * Time.deltaTime;
        }

        if (controller.isGrounded && ySpeed < 0)
        {
            ySpeed = -2f;
        }
        else
        {
            ySpeed -= Utils.gravity * Time.deltaTime;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f)) 
        {
            move = Vector3.ProjectOnPlane(move, hit.normal);
        }

        Vector3 finalMove = move * speed + Vector3.up * ySpeed;
        controller.Move(finalMove * Time.deltaTime);

        controller.Move(move * speed * Time.deltaTime);
    }

    private void StartAttack()
    {
        chargingAttack = true;
        if (weaponCoroutine == null)
        {
            weaponCoroutine = StartCoroutine(WeaponEnumerator(weaponChargeAnimation, true));
        }
    }

    private void EndAttack()
    {
        chargingAttack = false;
        if (weaponCoroutine == null)
        {
            weaponCoroutine = StartCoroutine(WeaponEnumerator(weaponHitAnimation, false));
        }
    }

    private IEnumerator WeaponEnumerator(SpriteAnimation spriteAnimation, bool needsEnd)
    {
        float timePerSprite = spriteAnimation.totalTime / spriteAnimation.sprites.Length;
        for (int i = 0; i < spriteAnimation.sprites.Length; i++)
        {
            weaponImage.sprite = spriteAnimation.sprites[i];
            yield return new WaitForSeconds(timePerSprite);
        }
        weaponCoroutine = null;
        if (needsEnd && !chargingAttack)
        {
            EndAttack();
        }
    }

    public void Hit(Weapon weapon)
    {
        //Determine damage
    }
}
