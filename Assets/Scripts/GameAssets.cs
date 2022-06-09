using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;

    public static GameAssets getInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    public Sprite pipeSprite;
    public Transform pfPipeBottom;
    public Transform pfPipeHead;
    public Transform pfGround;
    public Transform pfPipeCheckpoint;
}
