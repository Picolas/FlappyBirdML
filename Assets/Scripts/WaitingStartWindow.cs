using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingStartWindow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Show();
        Bird.GetInstance().OnStartPlay += WaitingStartWindowOnStartPlay;
    }

    private void WaitingStartWindowOnStartPlay(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    
    private void Show()
    {
        gameObject.SetActive(true);
    }
}
