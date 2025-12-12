using UnityEngine;

public class Key : MonoBehaviour
{
    public Vector2 unpressPos = new(-0.1f, -0.1f), pressedPos = new(-0.05f, -0.05f);
    public float timeBetween = 1f, timePressed = 0.5f;

    private void Start()
    {
        StartPressing();
    }

    private void OnEnable()
    {
        StartPressing();
    }

    private void StartPressing()
    {
        transform.localPosition = (Vector3)unpressPos;
        StartCoroutine(PressLoop());
    }

    private System.Collections.IEnumerator PressLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetween);

            transform.localPosition = (Vector3)pressedPos;

            yield return new WaitForSeconds(timePressed);

            transform.localPosition = (Vector3)unpressPos;
        }
    }
}
