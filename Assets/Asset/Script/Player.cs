using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private LevelController levelController;
    private ExpressionController expressionController;
    [SerializeField] private Transform FaceExp, RightHand, LeftHand, playerSprite;
    [SerializeField] private Fairy fairy;
    [SerializeField] private List<Sprite> HandSprites;

    private LevelComponent levelComponent;
    void Awake()
    {
        levelComponent = new(transform, levelController);
        expressionController = new(FaceExp, RightHand, LeftHand, GetComponent<PlayerMovement>(), fairy, HandSprites, playerSprite);
    }

    // Update is called once per frame
    void Update()
    {
        levelComponent.Update();
        expressionController.Update();
    }
}

public class ExpressionController
{
    private Vector2 FACE_POS_MOVE = new(0.15f, 0.15f), FACE_POS_STAND = new(0, 0.12f);
    private Vector2 RIGHT_HAND_MOVE = new(0.5f, -0.1f), RIGHT_HAND_STAND = new(0, -0.12f), RIGHT_HAND_CONTROL = new(0.8f, 0.1f);
    private float RIGHT_ROT_MOVE = -60, RIGHT_ROT_STAND = 0, RIGHT_ROT_CONTROL = -30;

    private float playerMaxRot = 10;
    private bool isRight = true, isTrueRight = true;

    private Transform FaceExp, RightHand, LeftHand, playerSprite;
    private PlayerMovement _player;
    private Fairy _fairy;
    private List<Sprite> Sprites;

    public ExpressionController(Transform Face, Transform Right, Transform Left, PlayerMovement playerMovement, Fairy fairy, List<Sprite> sprites, Transform sprite)
    {
        FaceExp = Face; RightHand = Right; LeftHand = Left;
        _player = playerMovement;
        _fairy = fairy;
        Sprites = sprites;
        playerSprite = sprite;
    }

    public void Update()
    {
        if (_player._moveInput.x == 0)
        {
            FaceExp.localPosition = FACE_POS_STAND;
            //
            if (_fairy.GetControlState == false)
            {
                RightHand.GetComponent<SpriteRenderer>().sprite = Sprites[0];
                RightHand.localPosition = RIGHT_HAND_STAND;
                RightHand.transform.eulerAngles = new Vector3(
                    RightHand.transform.eulerAngles.x,
                    RightHand.eulerAngles.y,
                    RIGHT_ROT_STAND
                );
            }
            else
            {
                RightHand.GetComponent<SpriteRenderer>().sprite = Sprites[1];
                RightHand.localPosition = RIGHT_HAND_CONTROL;
                RightHand.transform.eulerAngles = new Vector3(
                    RightHand.transform.eulerAngles.x,
                    RightHand.eulerAngles.y,
                    RIGHT_ROT_CONTROL * (isTrueRight ? -1 : 1)
                );
            }
            //
            playerSprite.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            FaceExp.localPosition = FACE_POS_MOVE;
            //
            if (_fairy.GetControlState == false)
            {
                RightHand.GetComponent<SpriteRenderer>().sprite = Sprites[0];
                RightHand.localPosition = RIGHT_HAND_MOVE;
                if (_player._moveInput.x == 1)
                {
                    isTrueRight = true;
                    //RightHand.parent.localScale = new(Mathf.Abs(RightHand.parent.localScale.x), RightHand.parent.localScale.y);
                }
                else if (_player._moveInput.x == -1)
                {
                    isTrueRight = false;
                    //RightHand.parent.localScale = new(-Mathf.Abs(RightHand.parent.localScale.x), RightHand.parent.localScale.y);
                    //RightHand.localPosition = RIGHT_HAND_MOVE * new Vector2(-1, 1);
                }
                RightHand.transform.eulerAngles = new Vector3(
                    RightHand.transform.eulerAngles.x,
                    RightHand.eulerAngles.y,
                    RIGHT_ROT_MOVE * ((_player._moveInput.x == 1) ? 1 : -1)
                );
            }
            else
            {
                if (_player._moveInput.x == 1)
                {
                    isTrueRight = true;
                }
                else if (_player._moveInput.x == -1)
                {
                    isTrueRight = false;
                }
                RightHand.GetComponent<SpriteRenderer>().sprite = Sprites[1];
                RightHand.localPosition = RIGHT_HAND_CONTROL;
                RightHand.transform.eulerAngles = new Vector3(
                    RightHand.transform.eulerAngles.x,
                    RightHand.eulerAngles.y,
                    RIGHT_ROT_CONTROL * (isTrueRight ? 1 : -1)
                );
            }
            //
            if ((isRight && _player._moveInput.x == 1) || (!isRight && _player._moveInput.x == -1))
            {
                if ((Mathf.Abs(playerSprite.eulerAngles.z) < playerMaxRot)
                    || 360 - Mathf.Abs(playerSprite.eulerAngles.z) < playerMaxRot)
                    playerSprite.Rotate(0, 0, -10f * Time.deltaTime);
            }
            else
            {
                isRight = _player._moveInput.x == 1;
                playerSprite.localEulerAngles = new Vector3(0, 0, 0);
            }
        }
    }
}

public class LevelComponent
{
    private const float PLAYER_RADIUS = 0.6f;
    private float POSITION_Y_DEATH = -5;

    private Transform _player;
    private LevelController levelController;

    private List<Checkpoint> checkpoints;
    private Checkpoint currentCheckpoint;
    private LayerMask checkpointMask, obstacleMask;

    private float checkCooldown = 1f;
    private float lastCheckedTime = -Mathf.Infinity;
    public LevelComponent(Transform player, LevelController levelController)
    {
        _player = player;
        this.levelController = levelController;
        checkpointMask = LayerMask.GetMask(GameSetting.CHECKPOINT_LAYER_MASK);
        obstacleMask = LayerMask.GetMask(GameSetting.OBSTACLE_LAYER_MASK);
        checkpoints = new();
    }

    public void Update()
    {
        CheckpointBehaviour();
        CheckDeath();
    }

    private void CheckDeath()
    {
        CheckFallDeath();
        CheckKiller();
    }

    private void CheckFallDeath()
    {
        if (_player.transform.position.y < POSITION_Y_DEATH) ResetPlayer();
    }

    private void CheckKiller()
    {
        bool hit = Physics2D.OverlapCircle(_player.position, PLAYER_RADIUS, obstacleMask);
        if (hit) ResetPlayer();
    }

    private void ResetPlayer()
    {
        _player.transform.position = currentCheckpoint.transform.position;
        levelController.ResetPower();
    }

    private void CheckpointBehaviour()
    {
        if (Time.time - lastCheckedTime < checkCooldown) return;

        Collider2D hit = Physics2D.OverlapCircle(_player.position, PLAYER_RADIUS, checkpointMask);
        if (hit == null) return;
        currentCheckpoint = hit.GetComponent<Checkpoint>();

        lastCheckedTime = Time.time;

        if (checkpoints.Contains(currentCheckpoint))
        {
            if (currentCheckpoint.GetStatus()) return;
        }
        else
        {
            checkpoints.Add(currentCheckpoint);
        }

        UpdateCheckpoint();
    }
    private void UpdateCheckpoint()
    {
        if (currentCheckpoint)
            foreach (Checkpoint checkpoint in checkpoints)
            {
                checkpoint.UnsetCheckpoint();
            }
        currentCheckpoint.SetCheckpoint();
    }
}