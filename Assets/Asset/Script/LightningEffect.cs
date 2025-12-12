using System.Collections;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    public GameObject[] objects = new GameObject[5];
    public bool random = false;

    public int intensity = 5;

    private Coroutine flashingCoroutine;

    void OnEnable()
    {
        StartFlashing();
    }

    void OnDisable()
    {
        StopFlashing();
    }

    public void StartFlashing()
    {
        if (flashingCoroutine == null)
            flashingCoroutine = StartCoroutine(FlashRoutine());
    }

    public void StopFlashing()
    {
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            flashingCoroutine = null;
        }

        HideAll();
    }

    public void ResetFlashing()
    {
        StopFlashing();
        StartFlashing();
    }

    private IEnumerator FlashRoutine()
    {
        if (intensity <= 10)
        {
            float interval = 1f / Mathf.Max(1, intensity), halfInterval = 0.1f;

            while (true)
            {
                HideAll();

                int randomIndex = Random.Range(0, objects.Length);
                if (objects[randomIndex] != null)
                    objects[randomIndex].SetActive(true);

                float waitTime = halfInterval;
                if (random) waitTime = Random.Range(waitTime * 0.75f, waitTime * 1.25f);
                yield return new WaitForSeconds(waitTime);
                if (objects[randomIndex] != null)
                    objects[randomIndex].SetActive(false);

                waitTime = interval - halfInterval;
                if (random) waitTime = Random.Range(waitTime * 0.75f, waitTime * 1.25f);
                yield return new WaitForSeconds(waitTime);
            }
        }
        else
        {
            float interval = 1f / Mathf.Max(1, intensity);

            HideAll();
            while (true)
            {
                if (Random.Range(0, 10) == 5) HideAll();
                StartCoroutine(FlashSingle(Random.Range(0, objects.Length), 0.1f));
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private IEnumerator FlashSingle(int randomIndex, float value)
    {
        if (objects[randomIndex] != null)
            objects[randomIndex].SetActive(true);
        yield return new WaitForSeconds(value);
        if (objects[randomIndex] != null)
            objects[randomIndex].SetActive(false);
    }

    private void HideAll()
    {
        foreach (var obj in objects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
