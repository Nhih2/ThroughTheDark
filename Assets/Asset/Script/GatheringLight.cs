using UnityEngine;

public class GatheringLight : MonoBehaviour
{
    [SerializeField] private GameObject Fairy, HomingOrbPrefab;
    private LayerMask playerMask;
    private float radius = 1;

    void Start()
    {
        playerMask = LayerMask.GetMask(GameSetting.PLAYER_LAYERMASK);
    }

    void Update()
    {
        CheckTouchPlayer();
    }

    private void CheckTouchPlayer()
    {
        bool hit = Physics2D.OverlapCircle(transform.position, radius, playerMask);
        if (hit) Explode();
    }

    public void Reset()
    {
        gameObject.SetActive(true);
    }

    public void Explode()
    {
        //Transform orb = Instantiate(HomingOrbPrefab).transform;
        //orb.position = transform.position;
        //orb.GetComponent<HomingOrb>().target = Fairy.transform;
        Fairy.GetComponent<Fairy>().UpgradeFairy();
        gameObject.SetActive(false);
    }
}
