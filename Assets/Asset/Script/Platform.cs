using UnityEngine;

public class Platform : MonoBehaviour
{
    private HorMover horMover;

    void Awake()
    {
        horMover = GetComponent<HorMover>();
    }

    public void Reset()
    {
        if (horMover) horMover.Reset();
    }
}
