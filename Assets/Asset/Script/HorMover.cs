using UnityEngine;

public class HorMover : MonoBehaviour
{
    public Vector2 moveDistance = new(2, 0);
    public float moveDuration = 2f;
    public float waitTime = 0.5f;

    private Vector2 originalPosition, startPosition, targetPosition;
    private float timer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        originalPosition = startPosition = transform.position;
        targetPosition = startPosition + moveDistance;
    }

    void Update()
    {
        if (isWaiting) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / moveDuration);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);
        transform.position = Vector2.Lerp(startPosition, targetPosition, smoothT);

        if (t >= 1f)
        {
            isWaiting = true;
            Invoke(nameof(SwapDirection), waitTime);
        }
    }

    public void Reset()
    {
        timer = 0;
        isWaiting = false;
        startPosition = originalPosition;
        targetPosition = startPosition + moveDistance;
    }

    void SwapDirection()
    {
        isWaiting = false;
        timer = 0f;

        Vector2 temp = startPosition;
        startPosition = targetPosition;
        targetPosition = temp;
    }
}
