using UnityEngine;

public class HomingOrb : MonoBehaviour
{
    public float speed = 5f;
    public float turnBackDelay = 1f;
    public int maxPasses = 2;
    public Transform target;
    public Vector2 initialDirection = Vector2.right;
    public float turnSpeed = 3f; // controls how sharply it turns

    private Vector2 moveDir;
    private int passCount = 0;
    private bool returning = false;
    private bool canDetectPass = false;

    void Start()
    {
        moveDir = initialDirection.normalized;
        Invoke(nameof(StartReturn), turnBackDelay);
    }

    void Update()
    {
        if (returning && target != null)
        {
            // Smoothly rotate toward the target
            Vector2 toTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
            moveDir = Vector2.Lerp(moveDir, toTarget, turnSpeed * Time.deltaTime);
        }

        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);

        // Detect passing the target
        if (returning && canDetectPass && target != null)
        {
            Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;

            // If the orb has passed the target (angle flips)
            if (Vector2.Dot(moveDir.normalized, toTarget.normalized) < 0f)
            {
                passCount++;
                canDetectPass = false;

                if (passCount >= maxPasses)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Go back to straight fly
                    returning = false;
                    moveDir = initialDirection.normalized;
                    Invoke(nameof(StartReturn), turnBackDelay);
                }
            }
        }
    }

    void StartReturn()
    {
        returning = true;
        canDetectPass = true;
    }
}
