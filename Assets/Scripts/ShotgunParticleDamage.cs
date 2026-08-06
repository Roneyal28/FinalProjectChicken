using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class ShotgunParticleDamage : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 1;
    private readonly HashSet<int> damagedTargets = new HashSet<int>();
    private int currentShotId;

    public void SetDamage(int amount)
    {
        damage = Mathf.Max(1, amount);
    }

    public void BeginShot(int shotId, int amount)
    {
        if (shotId != currentShotId)
        {
            currentShotId = shotId;
            damagedTargets.Clear();
        }

        SetDamage(amount);
    }

    private void OnParticleCollision(GameObject other)
    {
        Vector2 attackDirection = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

        WolfController wolf = other.GetComponentInParent<WolfController>();
        if (wolf != null)
        {
            if (!damagedTargets.Add(wolf.GetInstanceID()))
                return;

            wolf.TakeDamage(damage, attackDirection);
            return;
        }

        EnemyBehavior rat = other.GetComponentInParent<EnemyBehavior>();
        if (rat != null && damagedTargets.Add(rat.GetInstanceID()))
            rat.TakeDamage(damage, attackDirection);
    }
}
