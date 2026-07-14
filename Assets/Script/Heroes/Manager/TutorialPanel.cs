using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    public void Close()
    {
        gameObject.SetActive(false);
        pausePanel?.SetActive(true);
    }
}
