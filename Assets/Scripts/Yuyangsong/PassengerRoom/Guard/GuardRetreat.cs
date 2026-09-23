using System.Collections;
using UnityEngine;

public class GuardRetreat : MonoBehaviour
{
    public Transform retreatTarget;
    public Collider blockingCollider;
    [Min(0f)] public float reactionDuration = 0.25f;
    [Min(0.01f)] public float moveSpeed = 0.65f;
    [Min(1f)] public float turnSpeed = 150f;
    bool retreatStarted;

    public void Retreat()
    {
        if (retreatStarted || !isActiveAndEnabled || retreatTarget == null) return;
        retreatStarted = true;
        StartCoroutine(RetreatRoutine());
    }

    IEnumerator RetreatRoutine()
    {
        // A short pause, turn, and step aside; no Animator or navigation needed.
        yield return new WaitForSeconds(reactionDuration);
        Vector3 direction = retreatTarget.position - transform.position;
        direction.y = 0f;
        Quaternion facing = direction.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(direction, Vector3.up) : transform.rotation;
        while (Quaternion.Angle(transform.rotation, facing) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facing,
                Mathf.Max(1f, turnSpeed) * Time.deltaTime);
            yield return null;
        }
        while (Vector3.Distance(transform.position, retreatTarget.position) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, retreatTarget.position,
                Mathf.Max(0.01f, moveSpeed) * Time.deltaTime);
            yield return null;
        }
        transform.position = retreatTarget.position;
        if (blockingCollider != null) blockingCollider.enabled = false;
    }
}
