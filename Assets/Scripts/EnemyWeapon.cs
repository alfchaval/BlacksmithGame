using UnityEngine;

public class EnemyWeapon : Weapon
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player)
        {
            player.Hit(this);
        }
    }
}
