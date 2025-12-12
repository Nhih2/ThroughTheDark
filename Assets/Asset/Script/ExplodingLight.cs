using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExplodingLight : MonoBehaviour
{
    public Light2D light2D;
    public float explodeDuration = 0.5f;
    public float maxOuterRadius = 5f, originalRadius = 1.5f;
    public float maxIntensity = 2f, originalInstenisty = 0.5f;

    private float timer = 0f;
    private bool isExploding = false;

    private LayerMask playerMask;
    private float radius = 1;

    void Start()
    {
        playerMask = LayerMask.GetMask(GameSetting.PLAYER_LAYERMASK);
        if (light2D == null)
            light2D = GetComponent<Light2D>();
    }

    void Update()
    {
        ExplodeAnim();
        CheckTouchPlayer();
    }

    private void ExplodeAnim()
    {
        if (!isExploding) return;

        timer += Time.deltaTime;
        float t = timer / explodeDuration;

        float easedT = Mathf.SmoothStep(0f, 1f, t);

        light2D.pointLightOuterRadius = Mathf.Lerp(0f, maxOuterRadius, easedT);
        light2D.intensity = Mathf.Lerp(maxIntensity, 0f, easedT);

        if (t >= 1f)
        {
            gameObject.SetActive(false);
            isExploding = false;
        }
    }

    public void Reset()
    {
        gameObject.SetActive(true);
        timer = explodeDuration;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
        light2D.pointLightOuterRadius = originalRadius;
        light2D.intensity = originalInstenisty;
        isExploding = false;
    }

    private void CheckTouchPlayer()
    {
        if (isExploding) return;
        bool hit = Physics2D.OverlapCircle(transform.position, radius, playerMask);
        if (hit) Explode();
    }

    public void Explode()
    {
        timer = 0f;
        isExploding = true;

        light2D.intensity = maxIntensity;
        light2D.pointLightOuterRadius = 0f;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }
}
