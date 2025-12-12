using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject Checked, Unchecked;
    private bool isChecked = false;
    void Awake()
    {
        Checked.SetActive(false);
        Unchecked.SetActive(true);
    }
    public void SetCheckpoint()
    {
        isChecked = true;
        Checked.SetActive(true);
        Unchecked.SetActive(false);
    }
    public void UnsetCheckpoint()
    {
        isChecked = false;
        Checked.SetActive(false);
        Unchecked.SetActive(true);
    }
    public bool GetStatus() => isChecked;
}
