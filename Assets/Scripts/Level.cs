using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private const float CAMERA_ORTHO_SIZE = 50f;
    private const float PIPE_WIDTH = 0.26f;
    private const float PIPE_HEAD_HEIGHT = 4.5f;
    private const float PIPE_BOTTOM_SCALE = 39.06276f;
    private const float PIPE_MOVE_SPEED = 20f;
    private const float PIPE_DESTROY_X_POSITION = -100f;

    private List<Pipe> pipeList;

    private void Awake()
    {
          pipeList = new List<Pipe>(); 
    }

    void createDoublePipes(float y,  float size, float xPosition)
    {
        CreatePipe(y - size * 0.5f, xPosition, true); 
        CreatePipe(CAMERA_ORTHO_SIZE * 2f - y - size * 0.5f, xPosition, false); 
    }
     
    void CreatePipe(float height, float xPosition, bool createBottom) 
    {
        // Pipe head
        Transform pipeHead = Instantiate(GameAssets.getInstance().pfPipeHead);

        float pipeHeadYPosition;
        if (createBottom)
        {
            pipeHeadYPosition = -CAMERA_ORTHO_SIZE + height - PIPE_HEAD_HEIGHT * 0.5f; 
        }
        else
        {
            pipeHeadYPosition = +CAMERA_ORTHO_SIZE - height + PIPE_HEAD_HEIGHT * 0.5f;  
        }
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition);
        // Pipe bottom
        Transform pipeBottom = Instantiate(GameAssets.getInstance().pfPipeBottom);
        
        float pipeBottomYPosition;
        if (createBottom)
        { 
            pipeBottomYPosition = -CAMERA_ORTHO_SIZE;
        }
        else 
        {
            pipeBottomYPosition = +CAMERA_ORTHO_SIZE;
            pipeBottom.localScale = new Vector3(PIPE_BOTTOM_SCALE, -1f, PIPE_BOTTOM_SCALE);
        }
        pipeBottom.position = new Vector3(xPosition, pipeBottomYPosition );

        SpriteRenderer pipeBodySpriteRenderer = pipeBottom.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPE_WIDTH, height);
        
        BoxCollider2D pipeBottomBoxCollider = pipeBottom.GetComponent<BoxCollider2D>();
        pipeBottomBoxCollider.size = new Vector2(PIPE_WIDTH, height);
        pipeBottomBoxCollider.offset = new Vector2(0f, height *  0.5f);

        Pipe pipe = new Pipe(pipeHead, pipeBottom); 
        pipeList.Add(pipe);
    }
     
    // Start is called before the first frame update
    void Start()
    {
        createDoublePipes(50f, 20f, 20f);
        createDoublePipes(50f, 30f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        HandlePipeMovement();
    }

    void HandlePipeMovement() 
    {
        for (int i = 0; i < pipeList.Count; i++)
        {
            Pipe pipe = pipeList[i];
            pipe.Move();
            if (pipe.GetXPosition() < PIPE_DESTROY_X_POSITION)
            { 
                pipe.DestroySelf();
                pipeList.Remove(pipe);
                i--;
            }
        }
    }

    // SIngle pipe
    class Pipe
    {
        private Transform pipeHeadTransform;
        private Transform pipeBottomTransform;

        public Pipe(Transform pipeHeadTransform, Transform pipeBottomTransform )
        {
             this.pipeHeadTransform = pipeHeadTransform; 
             this.pipeBottomTransform = pipeBottomTransform;
        }

        public void Move()
        {
            pipeHeadTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime; 
            pipeBottomTransform .position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime; 
        }

        public float GetXPosition()
        { 
            return this.pipeBottomTransform.position.x;
        }

        public void DestroySelf()
        {
            Destroy(this.pipeHeadTransform.gameObject);
            Destroy(this.pipeBottomTransform.gameObject); 
        }
    }
}
