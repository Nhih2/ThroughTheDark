using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

//This is so damn spagetti
//Ah, how much debt did I accumulate with this???

public class LevelController : MonoBehaviour
{
    [SerializeField] private List<GameObject> BorderObjects, BorderObjects2, GameTileMap;
    [SerializeField] private Transform _player;
    [SerializeField] private List<Transform> FirstSpawnPoints;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private List<GameObject> powerHolder, platformHolder;
    [SerializeField] private Fairy fairy;
    [SerializeField] private List<GameObject> Tutorials;
    [SerializeField] private List<Button> Buttons;
    [SerializeField] private CameraForeground cameraForeground;
    [SerializeField] private Transform templeDoor;

    private readonly Vector2 PlayerMenuPosition = new(0, 97.5f);
    private readonly List<Vector2> LEVEL_BORDER_1 = new() { new(9.5f, 0), new(9f + 18.5f, 0), new(9f + 18.5f * 2f, 0), new(63f, 0), new(81, 0), new(98.3f, 0), new(116f, 0) };
    private readonly List<Vector2> LEVEL_BORDER_2 = new() { new(17.5f, 0), new(54f, 0), new(89f, 0), new(150f, 0), new(81, 0), new(98.3f, 0), new(120f, 0) };
    private readonly List<int> BORDER_DIR_HOR_1 = new() { 1, 1, 1, 1, 1, 1, 1 };
    private readonly List<int> BORDER_DIR_HOR_2 = new() { 1, 1, 1, 1, 1, 1, 1 };
    private readonly List<int> BORDER_DIR_VER_1 = new() { 0, 0, 0, 0, 0, 0, 0 };
    private readonly List<int> BORDER_DIR_VER_2 = new() { 0, 0, 0, 0, 0, 0, 0 };
    private readonly List<int> FINAL_LEVEL = new() { 6, 2 };

    private static List<bool> LevelPassed = new() { false, false };
    public static List<bool> TutorialComplete = new() { false, false, false, false, false };
    private List<SpriteRenderer> buttonRenderer;

    private int level = 0;
    public static int stage = 0;
    private Vector2 levelBorder = new();
    private int isBorderRight, isBorderUp;

    private PlayerMovement playerMovement;

    #region AUDIO
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> tracks, menuTracks;
    private List<AudioClip> currentTracks;
    private bool isPlaying = false;
    private int currentTrackIndex = 0;

    private void HandleAudio()
    {
        if (!audioSource.isPlaying && isPlaying)
        {
            NextTrack();
        }
    }
    private void StartPlaying()
    {
        PlayCurrentTrack();
        isPlaying = true;
    }
    private void StopPlaying()
    {
        audioSource.Stop();
        isPlaying = false;
    }
    private void ResetPlaying()
    {
        currentTrackIndex = 0;
        audioSource.time = 0f;
        if (stage == 0)
        {
            currentTracks = menuTracks;
            audioSource.volume = 0.15f;
        }
        else if (stage == 100) { }
        else
        {
            currentTracks = tracks;
            audioSource.volume = 0.1f;
        }
    }
    void PlayCurrentTrack()
    {
        audioSource.clip = currentTracks[currentTrackIndex];
        audioSource.Play();
    }
    void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % currentTracks.Count;
        PlayCurrentTrack();
    }
    #endregion


    void Awake()
    {
        playerMovement = _player.GetComponent<PlayerMovement>();
    }

    void Start()
    {
        buttonRenderer = new();
        foreach (Button button in Buttons)
        {
            button.eventHandler += ButtonListener;
            buttonRenderer.Add(button.GetComponent<SpriteRenderer>());
        }
        StartGame(0);
    }

    private void ButtonListener(object sender, Button.ButtonEventArgs e)
    {
        TutorialComplete[0] = true;
        StartGame(e.id);
    }

    public void StartGame(int stage)
    {
        LevelController.stage = stage;
        SetLevel();
        SetPlayer();
        SetTileMap();
        cameraMovement.StartGame();
        fairy.ResetZero();
        fairy.ResetOnDeath();
        level = 0;
        ResetPlaying();
        StartPlaying();
    }

    private void SetPlayer()
    {
        if (stage == 0)
        {
            playerMovement.SwitchOff();
            _player.position = PlayerMenuPosition;
        }
        else
        {
            playerMovement.SwitchOn();
            _player.position = FirstSpawnPoints[stage - 1].position;
        }
    }

    private void SetTileMap()
    {
        foreach (GameObject tilemap in GameTileMap)
        {
            tilemap.SetActive(false);
        }
        GameTileMap[stage].SetActive(true);
    }

    void Update()
    {
        CheckNextLevel();
        CheckButton();
        CheckTutorial();
        HandleAudio();
    }

    private void CheckButton()
    {
        int i = 0;
        foreach (SpriteRenderer renderer in buttonRenderer)
        {
            renderer.color = LevelPassed[i] ? Color.green : Color.black;
            i++;
        }
    }

    public void CompleteLevel()
    {
        LevelPassed[stage - 1] = true;
        StartGame(0);
    }

    private void CheckTutorial()
    {
        for (int i = 0; i < Tutorials.Count; i++)
            if (TutorialComplete[i] && Tutorials[i].activeSelf) Tutorials[i].SetActive(false);
    }

    private void CheckNextLevel()
    {
        if (stage == 0) return;
        if (level == -1) return;
        if (isBorderRight == 1 && _player.position.x > levelBorder.x) NextLevel();
        if (isBorderRight == -1 && _player.position.x < levelBorder.x) NextLevel();
        if (isBorderUp == 1 && _player.position.y > levelBorder.y) NextLevel();
        if (isBorderUp == -1 && _player.position.y < levelBorder.y) NextLevel();
    }

    private System.Collections.IEnumerator EnterTempleSequence()
    {
        level = -1;
        playerMovement.SwitchOff();
        cameraForeground.Activate();
        yield return new WaitForSeconds(1);
        cameraMovement.EnterTemple();
        playerMovement.transform.position = templeDoor.transform.position;
        yield return new WaitForSeconds(0.1f);
        cameraForeground.ReturnToOrigin();
        playerMovement.SwitchOn();
    }

    private void NextLevel()
    {
        if (stage == 0) return;
        if (stage == 1 && level == 0) TutorialComplete[1] = true;
        if (stage == 1 && level == 1) TutorialComplete[2] = true;
        if (stage == 1 && level == 4) TutorialComplete[3] = true;
        if (level >= FINAL_LEVEL[stage - 1])
        {
            //StartCoroutine(EnterTempleSequence());
            CompleteLevel();
            return;
        }
        if (stage == 1)
        {
            foreach (GameObject border in BorderObjects) border.SetActive(false);
            BorderObjects[level].SetActive(true);
        }
        else
        {
            foreach (GameObject border in BorderObjects2) border.SetActive(false);
            BorderObjects2[level].SetActive(true);
        }
        cameraMovement.NextLevel(level);

        level++;

        SetLevel();
        playerMovement.SwitchControllable();

        fairy.RecordFairyValue();
    }

    private void SetLevel()
    {
        if (stage == 0) return;
        if (stage == 1)
        {
            levelBorder = LEVEL_BORDER_1[level];
            isBorderRight = BORDER_DIR_HOR_1[level];
            isBorderUp = BORDER_DIR_VER_1[level];
        }
        else if (stage == 2)
        {
            levelBorder = LEVEL_BORDER_2[level];
            isBorderRight = BORDER_DIR_HOR_2[level];
            isBorderUp = BORDER_DIR_VER_2[level];
        }
    }

    public void ResetPower()
    {
        foreach (GameObject holder in powerHolder)
        {
            List<Consumable> consumables = holder.GetComponentsInChildren<Consumable>(true).ToList();
            foreach (Consumable consumable in consumables)
            {
                consumable.Reset();
            }
        }
        foreach (GameObject holder in platformHolder)
        {
            List<Platform> platforms = holder.GetComponentsInChildren<Platform>(true).ToList();
            foreach (Platform platform in platforms)
            {
                platform.Reset();
            }
        }
        fairy.ResetToPreviousValue();
        fairy.ResetOnDeath();
    }
}