using UnityEngine;

public class PlayerWeapon : Weapon
{
    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy)
        {
            enemy.Hit(this);
        }
    }
}
