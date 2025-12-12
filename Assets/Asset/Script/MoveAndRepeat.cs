using UnityEngine;
public class MoveAndRepeat : MonoBehaviour
{
    public Vector2 startPosition;
    public Vector2 endPosition;
    public float moveDuration = 1f;
    public float waitDuration = 1f;

    private float timer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        transform.position = startPosition;
    }

    void Update()
    {
        if (!isWaiting)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector2.Lerp(startPosition, endPosition, smoothT);

            if (t >= 1f)
            {
                isWaiting = true;
                timer = 0f;
                Invoke(nameof(Reset), waitDuration);
            }
        }
    }

    void Reset()
    {
        transform.position = startPosition;
        timer = 0f;
        isWaiting = false;
    }
}