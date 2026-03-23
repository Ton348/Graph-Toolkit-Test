using UnityEngine;

public class BuildingStatusWindow : MonoBehaviour
{
    public GameObject windowRoot;

    private void Awake()
    {
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }
    }

    public void ToggleWindow()
    {
        if (windowRoot == null)
        {
            return;
        }

        windowRoot.SetActive(!windowRoot.activeSelf);
    }

    public void Open()
    {
        if (windowRoot != null)
        {
            windowRoot.SetActive(true);
        }
    }

    public void Close()
    {
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }
    }
}
