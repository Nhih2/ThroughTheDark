using UnityEngine;

public class VerticalHover : MonoBehaviour
{
    public float height = 2f;
    public float moveDuration = 2f;
    public float waitTime = 1f;

    private Vector3 startPos;
    private Vector3 fromPos;
    private Vector3 toPos;

    private bool movingUp = true;
    private float timer = 0f;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        startPos = transform.position;
        fromPos = startPos;
        toPos = startPos + Vector3.up * height;
        moveDuration = Random.Range(1f, 3f);
    }

    void Update()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                movingUp = !movingUp;
                fromPos = movingUp ? startPos : startPos + Vector3.up * height;
                toPos = movingUp ? startPos + Vector3.up * height : startPos;

                timer = 0f;
                waitTimer = 0f;
                isWaiting = false;
            }
            return;
        }

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / moveDuration);

        float easedT = Mathf.SmoothStep(0f, 1f, t);
        transform.position = Vector3.Lerp(fromPos, toPos, easedT);

        if (t >= 1f)
        {
            isWaiting = true;
        }
    }
}
