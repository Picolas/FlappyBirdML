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
    private const float PIPE_MOVE_SPEED = 30f;
    private const float PIPE_DESTROY_X_POSITION = -100f;
    private const float PIPE_SPAWN_X_POSITION = +100f; //L"opposé de Destroy"
    private const float BIRD_X_POSITION = 0f;
    private static Level instance;
    private State state;

    public static Level GetInstance(){
        return instance;
    }

    private List<Pipe> pipeList;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float size; // temps entre les spawns
    private int pipesSpawned;
    private int pipesPassedCount;

    

    //Gérer la difficulté 
    public enum Difficulty {
        Easy,
        Medium,
        Hard, 
        Impossible,
    }

    public enum State {
        WaitingToStart,
        Playing,
        BirdDead,
    }
    
    private void Awake()
    {
          instance =  this;
          pipeList = new List<Pipe>(); 
          pipeSpawnTimerMax = 1f;
          SetDifficulty(Difficulty.Easy);
          state = State.WaitingToStart;
    }

    void Start()
    {
        //createDoublePipes(50f, 20f, 20f);
        //createDoublePipes(50f, 30f, 0f);
        Bird.GetInstance().OnDied += Bird_OnDied;
        Bird.GetInstance().OnStartedPlaying += Bird_OnStartedPlaying;
    }

    void Update()
    {
        if(state == State.Playing){
            HandlePipeMovement();
            HandlePipeSpawning();
        }
        
    }

    void createDoublePipes(float y,  float size, float xPosition)
    {
        CreatePipe(y - size * .5f, xPosition, true); 
        CreatePipe(CAMERA_ORTHO_SIZE * 2f - y - size * .5f, xPosition, false); 
        pipesSpawned++;
        Debug.Log(pipesSpawned);

        SetDifficulty(GetDifficulty());
    }

    private void SetDifficulty(Difficulty difficulty){
        switch(difficulty){
            case Difficulty.Easy:
            pipeSpawnTimerMax = 1.2f;
                size = 45f;
                break;
             case Difficulty.Medium:
                size = 40f;
                pipeSpawnTimerMax = 1.1f;
                break;
             case Difficulty.Hard:
                size = 35f;
                pipeSpawnTimerMax = 1.0f;
                break;
             case Difficulty.Impossible:
                size = 25f;
                pipeSpawnTimerMax = 0.9f;
                break;
        }
    }

    private Difficulty GetDifficulty(){
        if(pipesSpawned >= 30) return Difficulty.Impossible;
        if(pipesSpawned >= 20) return Difficulty.Hard;
        if(pipesSpawned >= 10) return Difficulty.Medium;
        return Difficulty.Easy;

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

        Pipe pipe = new Pipe(pipeHead, pipeBottom, createBottom); 
        pipeList.Add(pipe);
    }

    public int GetPipesSpawned(){
        return this.pipesSpawned;
    }

    public int GetPipesPassedCount(){
        return this.pipesPassedCount;
    }
    // Start is called before the first frame update
    
    private void Bird_OnStartedPlaying(object sender, System.EventArgs e){
        state = State.Playing;
        //yield return new WaitForSeconds(1) 
    }

    private void Bird_OnDied(object sender, System.EventArgs e){
        state = State.BirdDead;
        //yield return new WaitForSeconds(1)       
    }
    // Update is called once per frame
    

    void HandlePipeSpawning(){
        pipeSpawnTimer -= Time.deltaTime;
        if(pipeSpawnTimer < 0){
            //Le temps pour faire apparaitre un nouveau pipe
            pipeSpawnTimer += pipeSpawnTimerMax; 
            //On veut rendre aléatoire la hauteur des deux pipes opposés et les garder dans une certaine fourchette
            float heightEdgeLimit = 10f;
            float minHeight = size * .5f + heightEdgeLimit;
            float totalHeight =  CAMERA_ORTHO_SIZE * 2f;
            float maxHeight = totalHeight - size * .5f - heightEdgeLimit;
            float height = UnityEngine.Random.Range(minHeight, maxHeight);
            createDoublePipes(height  , size, PIPE_SPAWN_X_POSITION);
        }
    }

    void HandlePipeMovement() 
    {
        for (int i = 0; i < pipeList.Count; i++)
        {
            Pipe pipe = pipeList[i];
            bool isToTheRightOfBird = pipe.GetXPosition() > BIRD_X_POSITION;
            pipe.Move();
            if(isToTheRightOfBird && pipe.GetXPosition() <= BIRD_X_POSITION && pipe.IsBottom()){
                pipesPassedCount++;
            } 
            if (pipe.GetXPosition() < PIPE_DESTROY_X_POSITION)
            { 
                pipe.DestroySelf();
                pipeList.Remove(pipe);
                i--;
            }
        }
    }

    //Gérer la difficulté

    
    //On retourne le nombre de pipes passés pour calculer le score 
    

    // SIngle pipe
    class Pipe
    {
        private Transform pipeHeadTransform;
        private Transform pipeBottomTransform;
        private bool isBottom; 

        public Pipe(Transform pipeHeadTransform, Transform pipeBottomTransform, bool isBottom)
        {
             this.pipeHeadTransform = pipeHeadTransform; 
             this.pipeBottomTransform = pipeBottomTransform;
             this.isBottom = isBottom;
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

        public bool IsBottom(){
            return this.isBottom;
        }
        public void DestroySelf()
        {
            Destroy(this.pipeHeadTransform.gameObject);
            Destroy(this.pipeBottomTransform.gameObject); 
        }
    }
}
