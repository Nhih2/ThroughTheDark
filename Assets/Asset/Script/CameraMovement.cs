using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector2 ORIGINAL_POS = new(6.42f, 0), ORIGINAL_SIZE = new(1, 1);
    private Vector3 TEMPLE_POS = new(100, 100.5f, -10);
    private List<Vector3> CameraPosition = new() { new(0, 101, -10), new(0, 0, -10), new(0, 5, -10) };
    private List<float> UI_Scale = new() { 1, 1, 2 };

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject energyHolder, BackgroundHolder;
    private float moveDuration = 2f;
    private readonly List<Vector3> levelTarget_1 = new() { new(17.75f, 0, -10), new(17.75f * 2, 0, -10), new(17.75f * 3, 0, -10), new(17.75f * 4, 0, -10), new(17.75f * 5, 0, -10), new(17.75f * 6, 0, -10) };
    private readonly List<Vector3> levelTarget_2 = new() { new(17.75f * 2, 5, -10), new(17.75f * 2 * 2, 5, -10), new(17.75f * 2 * 3, 5, -10), new(17.75f * 2 * 4, 5, -10), new(17.75f * 5, 0, -10), new(17.75f * 6, 0, -10) };

    private Vector3 startPos;
    private Vector3 targetPos;
    private List<Vector3> levelTarget;
    private float elapsedTime = 0f;
    private bool isMoving = false, isSwitched = true;

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (LevelController.stage == 0)
        {
            GetComponent<Camera>().orthographicSize = 5f;
        }
        else if (LevelController.stage == 1)
        {
            levelTarget = levelTarget_1;
            GetComponent<Camera>().orthographicSize = 5f;
            energyHolder.transform.position = (Vector2)transform.position + ORIGINAL_POS;
        }
        else if (LevelController.stage == 2)
        {
            levelTarget = levelTarget_2;
            GetComponent<Camera>().orthographicSize = 10f;
            energyHolder.transform.position = (Vector2)transform.position + ORIGINAL_POS * 2;
        }
        energyHolder.transform.localScale = ORIGINAL_SIZE * UI_Scale[LevelController.stage];
        BackgroundHolder.transform.localScale = ORIGINAL_SIZE * UI_Scale[LevelController.stage];
        transform.position = CameraPosition[LevelController.stage];
    }

    public void EnterTemple()
    {
        GetComponent<Camera>().orthographicSize = 5f;
        energyHolder.transform.position = (Vector2)transform.position + ORIGINAL_POS;
        energyHolder.transform.localScale = ORIGINAL_SIZE * UI_Scale[1];
        BackgroundHolder.transform.localScale = ORIGINAL_SIZE * UI_Scale[1];
        transform.position = TEMPLE_POS;
    }

    public void NextLevel(int currentLevel)
    {
        startPos = transform.position;
        targetPos = levelTarget[currentLevel];
        elapsedTime = 0f;
        isMoving = true;
        isSwitched = false;
    }

    void Update()
    {
        if (!isMoving || LevelController.stage == 0) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / moveDuration);

        float easedT = 1f - Mathf.Pow(1f - t, 2f);

        transform.position = Vector3.Lerp(startPos, targetPos, easedT);

        if (t >= 1f)
        {
            isMoving = false;
            if (!isSwitched)
            {
                playerMovement.SwitchControllable();
                isSwitched = true;
            }
        }
    }
}
