using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public float boneColliderRadius = 0.08f;
    public float ragdollForceMultiplier = 1f;
    public bool setupOnAwake = true;

    public void SetupRagdoll()
    {
    }

    public void ActivateRagdoll(Vector3 forceOrigin, float forcePower, float hitRadius = 0.5f)
    {
    }
}
