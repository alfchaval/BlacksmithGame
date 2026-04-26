using UnityEngine;

public class Mineral : MonoBehaviour
{
    private bool hasMaterial;

    public void Hit(Weapon weapon)
    {
        if (hasMaterial)
        {
            hasMaterial = false;
            //animación
        }
    }
}
