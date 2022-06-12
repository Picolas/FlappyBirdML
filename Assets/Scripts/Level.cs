using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private const float CAM_ORTHO_SIZE = 50f;
    private const float PIPE_WIDTH = 0.26f;
    private const float PIPE_HEAD_HEIGHT = 4.5f;
    private const float PIPE_BOTTOM_SCALE = 39.06276f;
    private const float PIPE_MOVE_SPEED = 30f;
    private const float PIPE_DESTROY_X_POSITION = -100f;
    private const float PIPE_SPAWN_X_POSITION = +100f; //L"opposé de Destroy"
    private const float GROUND_DESTROY_X_POSITION = -195.5f;
    private const float GROUND_SPAWN_X_POSITION = +100f;
    private const float BIRD_X_POSITION = 0f;
    private static Level instance;
    private State state;
    
    public event EventHandler OnPipePassed;

    public static Level GetInstance(){
        return instance;
    }

    private List<Transform> groundList;
    private List<Pipe> pipeList;
    private float pSpawnTimer;
    private float pSpawnTimerMax;
    private float size; // temps entre les spawns
    private int pSpawned;
    private int pPassedCount;
    private List<PipeFull> pipeFullList;

    

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
          pipeFullList = new List<PipeFull>();
          SpawnFirstGround();
          pSpawnTimerMax = 1f;
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
            ManagePipeMovement();
            ManagePipeSpawning();
            ManageGroundMove();
        }
        
    }

    void createDoublePipes(float y,  float size, float xPosition)
    {
        Pipe pipeBottom = CreatePipe(y - size * .5f, xPosition, true); 
        Pipe pipeHead = CreatePipe(CAM_ORTHO_SIZE * 2f - y - size * .5f, xPosition, false); 
        pSpawned++;
        Debug.Log(pSpawned);
        
        pipeFullList.Add(new PipeFull {
            pipeTop = pipeHead,
            pipeBottom = pipeBottom,
            y = y,
            size = size,
        });

        SetDifficulty(GetDifficulty());
    }

    private void SpawnFirstGround()
    {
        groundList = new List<Transform>();
        Transform ground;
        float groundY = -52.9f;
        float groundWidth = 193.9f;
        ground = Instantiate(GameAssets.getInstance().pfGround, new Vector3(0, groundY, 0), Quaternion.identity);
        groundList.Add(ground);
        ground = Instantiate(GameAssets.getInstance().pfGround, new Vector3(groundWidth, groundY, 0), Quaternion.identity);
        groundList.Add(ground);
        ground = Instantiate(GameAssets.getInstance().pfGround, new Vector3(groundWidth * 2f, groundY, 0), Quaternion.identity);
        groundList.Add(ground);
    }
    
    private void ManageGroundMove()
    {
        foreach (Transform ground in groundList)
        {
            ground.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime;

            if (ground.position.x < GROUND_DESTROY_X_POSITION)
            {
                float rightXPosition = -100f;
                for (int i = 0; i < groundList.Count; i++)
                {
                    if (groundList[i].position.x > rightXPosition)
                    {
                        rightXPosition = groundList[i].position.x;
                    }
                }
                
                float groundWidth = 193.9f;
                ground.position = new Vector3(rightXPosition + groundWidth, ground.position.y, ground.position.z);
            }
        }
    }

    private void SetDifficulty(Difficulty difficulty){
        switch(difficulty){
            case Difficulty.Easy:
            pSpawnTimerMax = 1.2f;
                size = 45f;
                break;
             case Difficulty.Medium:
                size = 40f;
                pSpawnTimerMax = 1.1f;
                break;
             case Difficulty.Hard:
                size = 35f;
                pSpawnTimerMax = 1.0f;
                break;
             case Difficulty.Impossible:
                size = 25f;
                pSpawnTimerMax = 0.9f;
                break;
        }
    }

    private Difficulty GetDifficulty(){
        if(pSpawned >= 30) return Difficulty.Impossible;
        if(pSpawned >= 20) return Difficulty.Hard;
        if(pSpawned >= 10) return Difficulty.Medium;
        return Difficulty.Easy;

    } 

    Pipe CreatePipe(float height, float xPosition, bool createBottom) 
    {
        // Pipe head
        Transform pipeHead = Instantiate(GameAssets.getInstance().pfPipeHead);

        float pipeHeadYPosition;
        if (createBottom)
        {
            pipeHeadYPosition = -CAM_ORTHO_SIZE + height - PIPE_HEAD_HEIGHT * 0.5f; 
        }
        else
        {
            pipeHeadYPosition = +CAM_ORTHO_SIZE - height + PIPE_HEAD_HEIGHT * 0.5f;  
        }
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition);
        // Pipe bottom
        Transform pipeBottom = Instantiate(GameAssets.getInstance().pfPipeBottom);
        
        float pipeBottomYPosition;
        if (createBottom)
        { 
            pipeBottomYPosition = -CAM_ORTHO_SIZE;
        }
        else 
        {
            pipeBottomYPosition = +CAM_ORTHO_SIZE;
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
        
        // On créé le checkpoint au milieu des deux pipes
        if (createBottom) {
            Transform pipeCheckpoint = Instantiate(GameAssets.getInstance().pfPipeCheckpoint);
            pipeCheckpoint.localScale = new Vector3(.1f, size);
            pipeCheckpoint.parent = pipeBottom;
            pipeCheckpoint.localPosition = new Vector3(0, height + size * .5f);
        }

        return pipe;
    }

    public int GetPipesSpawned(){
        return this.pSpawned;
    }

    public int GetPipesPassedCount(){
        return this.pPassedCount;
    }

    private void Bird_OnStartedPlaying(object sender, System.EventArgs e){
        state = State.Playing;
        //yield return new WaitForSeconds(1) 
    }

    private void Bird_OnDied(object sender, System.EventArgs e){
        state = State.BirdDead;
        //yield return new WaitForSeconds(1)       
    }


    void ManagePipeSpawning(){
        pSpawnTimer -= Time.deltaTime;
        if(pSpawnTimer < 0){
            //Le temps pour faire apparaitre un nouveau pipe
            pSpawnTimer += pSpawnTimerMax; 
            //On veut rendre aléatoire la hauteur des deux pipes opposés et les garder dans une certaine fourchette
            float heightEdgeLimit = 10f;
            float minHeight = size * .5f + heightEdgeLimit;
            float totalHeight =  CAM_ORTHO_SIZE * 2f;
            float maxHeight = totalHeight - size * .5f - heightEdgeLimit;
            float height = UnityEngine.Random.Range(minHeight, maxHeight);
            createDoublePipes(height  , size, PIPE_SPAWN_X_POSITION);
        }
    }

    void ManagePipeMovement() 
    {
        for (int i = 0; i < pipeList.Count; i++)
        {
            Pipe pipe = pipeList[i];
            bool isToTheRightOfBird = pipe.GetXPosition() > BIRD_X_POSITION;
            pipe.Move();
            if(isToTheRightOfBird && pipe.GetXPosition() <= BIRD_X_POSITION && pipe.IsBottom()){
                pPassedCount++;
                OnPipePassed?.Invoke(this, EventArgs.Empty);
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
    public class Pipe
    {
        public Transform pipeHeadTransform;
        public Transform pipeBottomTransform;
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
    
    public class PipeFull {

        public Pipe pipeBottom;
        public Pipe pipeTop;
        public float y;
        public float size;

    }
    
    // Fonction mlAgent
    private PipeFull GetPipeFullWithPipe(Pipe pipe) {
        for (int i = 0; i < pipeFullList.Count; i++) {
            PipeFull pipeFull = pipeFullList[i];
            if (pipeFull.pipeBottom == pipe || pipeFull.pipeTop == pipe) {
                return pipeFull;
            }
        }
        return null;
    }
    
    public PipeFull GetNextPipeFull() {
        for (int i = 0; i < pipeList.Count; i++) {
            Pipe pipe = pipeList[i];
            if (pipe.pipeBottomTransform != null && pipe.GetXPosition() > BIRD_X_POSITION && pipe.IsBottom()) {
                PipeFull pipeFull = GetPipeFullWithPipe(pipe);
                return pipeFull;
            }
        }
        return null;
    }
}
