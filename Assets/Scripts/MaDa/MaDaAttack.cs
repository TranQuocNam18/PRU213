using UnityEngine;

public class MaDaAttack : MonoBehaviour
{
    public float damage = 15f;
    public float attackCooldown = 1.5f;

    float nextAttackTime = 0f;

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextAttackTime)
            {
                PlayerHealth hp = collision.gameObject.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(damage);
                }

                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }
}
