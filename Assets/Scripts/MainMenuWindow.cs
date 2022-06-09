using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuWindow : MonoBehaviour {

    private void Awake() {
        UnityEngine.Events.UnityAction action = () => {UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");};
        transform.Find("playButton").GetComponent<Button>().onClick.AddListener(action);
        UnityEngine.Events.UnityAction action2 = () => {Application.Quit();};
        transform.Find("quitButton").GetComponent<Button>().onClick.AddListener(action2);
    }
}