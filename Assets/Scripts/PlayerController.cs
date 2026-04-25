using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviourInstance<PlayerController>
{
    [Header("Settings")]
    public float speed = 5f;
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
