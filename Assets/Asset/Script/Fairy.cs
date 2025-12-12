using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Fairy : MonoBehaviour
{
    [SerializeField] private List<Transform> Orbs;
    [SerializeField] private Transform fairyOrigin;
    [SerializeField] private Transform player;
    [SerializeField] private LightningEffect lightningEffect;
    [SerializeField] private AudioSource audioSource;
    private FairyMover fairyMover;
    private FairyLight fairyLight;

    void Awake()
    {
        fairyMover = new(fairyOrigin, gameObject.transform, player);
        fairyLight = new(gameObject.transform);
    }

    void Update()
    {
        fairyMover.Update();
        fairyLight.Update();
        SetFairyLevel();
        SetLightningEffect();
        if (fairyMover.isControl && LevelController.stage == 0 && !fairyLight.isExpanding)
        {
            fairyLight.Explode(FairyLight.ExpandMode.MenuExpand);
        }
        handleAudio();
    }

    private void handleAudio()
    {
        if ((fairyMover.isControl || fairyLight.isExpanding) && !audioSource.isPlaying)
        {
            if (fairyLight.isExpanding) audioSource.volume = 0.12f;
            else if (fairyMover.isControl) audioSource.volume = 0.05f;
            audioSource.Play();
        }
        else if (!(fairyMover.isControl || fairyLight.isExpanding) && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void SetLightningEffect()
    {
        if (fairyLight.isExpanding || fairyLight.isReturning)
        {
            if (lightningEffect.intensity != 15)
            {
                lightningEffect.intensity = 15;
                lightningEffect.ResetFlashing();
            }
            return;
        }
        else if (fairyMover.isControl)
        {
            if (lightningEffect.intensity != 5)
            {
                lightningEffect.intensity = 5;
                lightningEffect.ResetFlashing();
            }
            return;
        }
        else if (!fairyMover.isControl)
        {
            if (lightningEffect.intensity != 2)
            {
                lightningEffect.intensity = 2;
                lightningEffect.ResetFlashing();
            }
            return;
        }
    }

    private void SetFairyLevel()
    {
        for (int i = 0; i <= 10; i++)
        {
            if (fairyLight.lightPower > i) Orbs[i].gameObject.SetActive(true);
            else Orbs[i].gameObject.SetActive(false);
        }
    }

    public bool GetControlState => fairyMover.isControl;

    #region FAIRY INTERFACE
    int recordedLevel = 0;
    public void UpgradeFairy()
    {
        if (fairyLight.lightPower >= 10) return;
        fairyLight.UpdateLevel(fairyLight.lightPower + 1);
        fairyLight.Explode(FairyLight.ExpandMode.CollectDust);
    }

    public void RecordFairyValue()
    {
        recordedLevel = fairyLight.lightPower;
    }

    public void ResetToPreviousValue()
    {
        fairyLight.UpdateLevel(recordedLevel);
    }

    public void ResetZero()
    {
        fairyLight.UpdateLevel(0);
        recordedLevel = 0;
    }

    public void ResetOnDeath()
    {
        fairyMover.ControlReset();
    }
    #endregion
}

public class FairyLight
{
    private Light2D light2D;

    public int lightPower { get; private set; } = 0;
    private float baseInner = 0.2f, incInner = 0.1f;
    private float baseOuter = 2f, incOuter = 0.25f;
    private float innerRadius = 0.2f;
    private float outerRadius = 1.2f;

    public bool isExpanding, isReturning;
    public FairyLight(Transform fairy)
    {
        if (light2D == null)
            light2D = fairy.GetComponentInChildren<Light2D>();

        light2D.pointLightInnerRadius = innerRadius;
        light2D.pointLightOuterRadius = outerRadius;

        isExpanding = false; isReturning = false;
    }

    public void Update()
    {
        LightAnimation();
        ExplodeAnim();
        ExplodeControl();
    }

    #region LIGHT ACTIVATION
    private const float COLLECT_DURATION = 0.5f, ACTIVATE_DURATION = 1f, MENU_DURATION_1 = 1f, MENU_DURATION_2 = 2f, MENU_DURATION_3 = 3f;
    private float explodeDuration = 1f;
    private float startOuterRadius, targetOuterRadius, originalOuterRadius;
    private float timer = 0f;
    private bool isMenuStart = false;

    public enum ExpandMode
    {
        CollectDust,
        Activate,
        MenuExpand
    }
    private ExpandMode currentMode;

    public void Explode(ExpandMode mode)
    {
        if (isExpanding) return;

        currentMode = mode;
        timer = 0f;
        isExpanding = true;
        isReturning = false;
        startOuterRadius = light2D.pointLightOuterRadius;

        if (mode == ExpandMode.Activate)
        {
            lightPower--;
            explodeDuration = ACTIVATE_DURATION;
            targetOuterRadius = startOuterRadius + 7.5f;
        }
        else if (mode == ExpandMode.CollectDust)
        {
            light2D.color = Color.cyan;
            targetOuterRadius = startOuterRadius + 2.5f;
            explodeDuration = COLLECT_DURATION;
        }
        else if (mode == ExpandMode.MenuExpand)
        {
            targetOuterRadius = 0;
            originalOuterRadius = startOuterRadius;
            explodeDuration = MENU_DURATION_1;
            isMenuStart = false;
        }
    }

    private void ExplodeControl()
    {
        if (Input.GetKeyDown(KeyCode.E) && lightPower > 0)
        {
            Explode(ExpandMode.Activate);
        }
    }

    private void ExplodeAnim()
    {
        if (!isExpanding) return;

        timer += Time.deltaTime;
        float t = timer / explodeDuration;

        float easedT = Mathf.SmoothStep(0f, 1f, t);

        light2D.pointLightOuterRadius = Mathf.Lerp(startOuterRadius, targetOuterRadius, easedT);

        if (t >= 1f)
        {
            if (currentMode == ExpandMode.MenuExpand)
            {
                if (!isMenuStart)
                {
                    isMenuStart = true;
                    timer = 0;
                    targetOuterRadius = 25;
                    startOuterRadius = light2D.pointLightOuterRadius;
                    explodeDuration = MENU_DURATION_2;
                }
                else if (!isReturning)
                {
                    isReturning = true;
                    timer = 0;
                    targetOuterRadius = originalOuterRadius;
                    startOuterRadius = light2D.pointLightOuterRadius;
                    explodeDuration = MENU_DURATION_3;
                }
                else
                {
                    isExpanding = false;
                    isReturning = false;
                    isMenuStart = false;
                    light2D.color = Color.white;
                }
            }
            else
            {
                if (!isReturning)
                {
                    isReturning = true;
                    timer = 0;
                    targetOuterRadius = startOuterRadius;
                    startOuterRadius = light2D.pointLightOuterRadius;
                }
                else
                {
                    isExpanding = false;
                    isReturning = false;
                    light2D.color = Color.white;
                }
            }
        }
    }
    #endregion

    private void LightAnimation()
    {
        if (!isExpanding)
        {
            SetLightningToLevel();
            float t = Mathf.Sin(Time.time) * 0.25f + 0.5f;
            light2D.pointLightInnerRadius = Mathf.Lerp(innerRadius * 0.9f, innerRadius * 1.1f, t);
            light2D.pointLightOuterRadius = Mathf.Lerp(outerRadius * 0.9f, outerRadius * 1.1f, t);
        }
    }

    private void SetLightningToLevel()
    {
        innerRadius = baseInner + incInner * lightPower;
        outerRadius = baseOuter + incOuter * lightPower;
    }

    public void UpdateLevel(int level)
    {
        lightPower = level;
        SetLightningToLevel();
    }
}

public class FairyMover
{
    private Transform _fairy, _origin, _player;

    private float distance = 0.4f, moveDuration = 2f, waitTime = 0.5f;
    private float waitTimer = 0;

    private Vector3 startPos;
    private Vector3 leftTarget, rightTarget;
    private Vector3 fromPos, toPos;
    private bool movingLeft = true;
    private bool isWaiting = false;

    private float timer = 0f;

    public FairyMover(Transform fairyOrigin, Transform fairy, Transform player)
    {
        _player = player;
        _fairy = fairy;
        _origin = fairyOrigin;
        startPos = _origin.position;
        leftTarget = startPos + Vector3.left * distance;
        rightTarget = startPos + Vector3.right * distance;
    }

    public void Update()
    {
        AutoMove();
        Control();
    }

    #region FAIRY CONTROL
    public bool isControl { get; private set; }
    public bool isReturn { get; private set; }
    private bool isRight;
    private float travelDistance, maxDistance = 5, speed = 5, returnDuration = 1;
    private float menuTimer = 0, menuMaxTimer = 6, maxMenuDistance = 2, menuTravelTime = 2.99f;
    private float previousPosition = 0;
    private void Control()
    {
        if (LevelController.stage == 0)
        {
            if (!isControl)
            {
                if (menuTimer < menuMaxTimer) menuTimer += Time.deltaTime;
                else
                {
                    previousPosition = 0;
                    _fairy.position = _origin.position;
                    isControl = true;
                    timer = 0;
                }
            }
            else if (!isReturn)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / menuTravelTime);

                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float currentPosition = eased * maxMenuDistance;

                float delta = currentPosition - previousPosition;
                previousPosition = currentPosition;

                _fairy.Translate(Vector2.up * delta);

                if (t >= 1f)
                {
                    previousPosition = 0;
                    isReturn = true;
                    timer = 0;
                }
            }
            else
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / menuTravelTime);

                float eased = Mathf.SmoothStep(0f, 1f, t);
                float currentPosition = eased * maxMenuDistance;

                float delta = currentPosition - previousPosition;
                previousPosition = currentPosition;

                _fairy.Translate(Vector2.down * delta);

                if (t >= 1f)
                {
                    previousPosition = 0;
                    isReturn = false;
                    isControl = false;
                    menuTimer = 0;
                    timer = moveDuration / 2f;
                }
            }
        }
        else
        {
            if (LevelController.stage > 1)
            {
                if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Q))
                {
                    if (isReturn) return;
                    if (isControl)
                    {
                        isControl = false;
                        isReturn = true;
                        timer = 0;
                        moveDuration = returnDuration;
                        fromPos = _fairy.position;
                        toPos = _origin.position;
                        _fairy.SetParent(_origin.parent);
                    }
                    else
                    {
                        isControl = true;
                        isRight = _player.transform.localScale.x == 1;
                        travelDistance = 0;
                        _fairy.SetParent(null);
                    }
                }
                if (Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.Q))
                {
                    if (isReturn) return;
                    if (travelDistance > maxDistance) return;
                    int dir = isRight ? 1 : -1;
                    Vector2 travel = Vector2.right * dir * Time.deltaTime;
                    travelDistance += Mathf.Abs(travel.x);
                    _fairy.Translate(travel * speed);
                }
            }
        }
    }
    #endregion

    public void ControlReset()
    {
        _fairy.position = _origin.position;
        _fairy.SetParent(_origin.parent);
        isControl = false;
        isReturn = false;
    }

    private void AutoMove()
    {
        if (isWaiting || isControl) return;
        timer += Time.deltaTime;
        float t = timer / moveDuration;
        Recalculating();

        float easedT = Mathf.SmoothStep(0f, 1f, t);

        _fairy.transform.position = Vector3.Lerp(fromPos, toPos, easedT);

        if (t >= 1f && !isReturn)
        {
            SwitchDirectionAfterWait();
        }
        else if (t >= 1 && isReturn)
        {
            timer = 0.5f;
            isReturn = false;
            waitTimer = 0;
        }
    }

    private void Recalculating()
    {
        if (!isReturn)
        {
            startPos = _origin.position;
            leftTarget = startPos + Vector3.left * distance;
            rightTarget = startPos + Vector3.right * distance;
            fromPos = movingLeft ? rightTarget : leftTarget;
            toPos = movingLeft ? leftTarget : rightTarget;
        }
        else
        {
            toPos = _origin.position;
        }
    }

    private void SwitchDirectionAfterWait()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer > waitTime)
        {
            movingLeft = !movingLeft;
            fromPos = movingLeft ? rightTarget : leftTarget;
            toPos = movingLeft ? leftTarget : rightTarget;

            timer = 0;
            waitTimer = 0;
        }
    }
}