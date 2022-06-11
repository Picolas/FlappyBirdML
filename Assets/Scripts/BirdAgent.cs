using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BirdAgent : Agent
{
    
    [SerializeField] private Level level;
    private Bird bird;
    private bool jumpButtonActivated;
    
    void Awake() {
        bird = GetComponent<Bird>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        bird.OnDied += Bird_OnDied;
        level.OnPipePassed += Level_OnPipePassed;
    }
    
    private void Level_OnPipePassed(object sender, System.EventArgs e) {
        AddReward(2f);
    }
    
    void Bird_OnDied(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            jumpButtonActivated = true;
        }
    }

    public override void OnEpisodeBegin()
    {
        bird.Reset();
    }

   
    public override void CollectObservations(VectorSensor sensor)
    {
        float gameHeight = 100f;
        float birdHeight = (bird.transform.position.y + (gameHeight / 2f)) / gameHeight;

        //Debug.Log("birdHeight" + birdHeight);
        // On renvoit la hauteur du bird
        sensor.AddObservation(birdHeight);

        float pipeSpawnXPosition = 100f;
        // On recupère la prochaine pipe haut et bas
        Level.PipeFull pipeFull = level.GetNextPipeFull();
        
        if (pipeFull != null && pipeFull.pipeBottom != null && pipeFull.pipeBottom.pipeBottomTransform != null) {
            // On renvoit le milieu du pipe (là ou il faut aller)
            sensor.AddObservation(pipeFull.pipeBottom.GetXPosition() / pipeSpawnXPosition);
        } else
        {
            Debug.Log("test else");
            // Sinon on renvoit le middle
            sensor.AddObservation(0.5f);//1f
        }
        
        // On renvoit le milieu du pipe (là ou il faut aller)
        //sensor.AddObservation(pipeFull.pipeBottom.GetXPosition() / pipeSpawnXPosition);
        
        // On renvoit la largeur du bird
        sensor.AddObservation(bird.GetVelocityY() / 200f);
    }
    
    
    public override void OnActionReceived(ActionBuffers actions) {
        // on le recompense si il fait une action
        //AddReward(0.2f);

        Debug.Log("Discrete action 0 : " + actions.DiscreteActions[0]);
        
        if (actions.DiscreteActions[0] == 1)
        {
            Debug.Log("AI Jump");
            bird.Jump();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint")) {
            Debug.Log("Checkpoint AI");
            // ON le récompense si il passe une pipe
            AddReward(2f);
        } else {
            
            Debug.Log("Collision AI");
            // On le sanctionne si il ne passe pas
            SetReward(-1f);
            EndEpisode();
            
        }
    }
    
    
    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (jumpButtonActivated)
        {
            discreteActions[0] = 1;
        } else
        {
            discreteActions[0] = 0;
        }

        jumpButtonActivated = false;
    }
}
