using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverWindow : MonoBehaviour
{
    private Text scoreText;

    private void Awake(){
        scoreText = transform.Find("scoreText").GetComponent<Text>();
        UnityEngine.Events.UnityAction action = () => {UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");};
        transform.Find("RetryButton").GetComponent<Button>().onClick.AddListener(action);
        UnityEngine.Events.UnityAction action2 = () => {UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");};
        transform.Find("MainMenuButton").GetComponent<Button>().onClick.AddListener(action2);
    }

    private void Start(){
        Bird.GetInstance().OnDied += Bird_OnDied;
        Hide();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Retry
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    private void Bird_OnDied(object sender, System.EventArgs e){
        scoreText.text =  Level.GetInstance().GetPipesPassedCount().ToString();
        Show();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }
}
