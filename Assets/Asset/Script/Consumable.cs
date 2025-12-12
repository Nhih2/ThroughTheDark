using UnityEngine;

public class Consumable : MonoBehaviour
{
    private ExplodingLight explodingLight;
    private GatheringLight gatheringLight;

    void Awake()
    {
        explodingLight = GetComponent<ExplodingLight>();
        gatheringLight = GetComponent<GatheringLight>();
    }

    public void Reset()
    {
        if (explodingLight) explodingLight.Reset();
        if (gatheringLight) gatheringLight.Reset();
    }
}
