using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Score
{
    public static void Start()
    {
        Bird.GetInstance().OnDied += Bird_OnDied;
    }

    private static void Bird_OnDied(object sender, System.EventArgs e)
    {
        SetHighscore(Level.GetInstance().GetPipesPassedCount());
    }
    
    public static int GetHighscore()
    {
        return PlayerPrefs.GetInt("highscore");
    }

    public static bool SetHighscore(int score)
    {
        int highscore = GetHighscore();

        if (score > highscore)
        {
            PlayerPrefs.SetInt("highscore", score);
            PlayerPrefs.Save();

            return true;
        }
        return false;
    }
}
