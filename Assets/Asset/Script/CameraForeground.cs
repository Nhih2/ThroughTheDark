using UnityEngine;

public class CameraForeground : MonoBehaviour
{
    public float flyDistance = 20f;
    public float flySpeed = 20f;
    public float returnSpeed = 20f;

    private Vector3 originalPosition;
    private Vector3 firstTarget;
    private Vector3 secondTarget;

    private Vector3 originalScale;

    private enum State { Idle, FlyingOut, Waiting, FlyingFurther, Returning }
    private State currentState = State.Idle;

    private void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        firstTarget = originalPosition + Vector3.left * flyDistance;
        secondTarget = originalPosition + Vector3.left * flyDistance * 2f;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.FlyingOut:
                MoveToTarget(firstTarget, flySpeed, State.Waiting);
                break;

            case State.FlyingFurther:
                MoveToTarget(secondTarget, flySpeed, State.Returning);
                break;

            case State.Returning:
                transform.position = originalPosition;
                currentState = State.Idle;
                break;
        }
    }

    private void MoveToTarget(Vector3 target, float speed, State nextState)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            currentState = nextState;
        }
    }

    public void Activate()
    {
        if (LevelController.stage == 1) transform.localScale = originalScale;
        if (LevelController.stage == 2) transform.localScale = originalScale * 2;
        if (currentState == State.Idle)
        {
            currentState = State.FlyingOut;
        }
    }

    public void ReturnToOrigin()
    {
        if (currentState == State.Waiting)
        {
            currentState = State.FlyingFurther;
        }
    }
}
