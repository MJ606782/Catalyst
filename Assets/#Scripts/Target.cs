using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f;
    public bool isDead = false;

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;
        health -= amount;
        if (health <= 0f)
        {
            isDead = true;
            Die();
        }
    }

    public virtual void OnHit(Vector3 hitPoint, Vector3 hitNormal, float damageAmount)
    {
        TakeDamage(damageAmount);
    }

    protected virtual void Die()
    {
        Destroy(gameObject, 0.3f);
    }
}
