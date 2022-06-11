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
    private bool isJumpInputDown;
    
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
        AddReward(1f);
    }
    
    void Bird_OnDied(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            isJumpInputDown = true;
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
        sensor.AddObservation(birdHeight);

        float pipeSpawnXPosition = 100f;
        Level.PipeCompleted pipeComplete = level.GetNextPipeComplete();
        if (pipeComplete != null && pipeComplete.pipeBottom != null && pipeComplete.pipeBottom.pipeBottomTransform != null) {
            sensor.AddObservation(pipeComplete.pipeBottom.GetXPosition() / pipeSpawnXPosition);
        } else {
            sensor.AddObservation(1f);
        }

        sensor.AddObservation(bird.GetVelocityY() / 200f);
    }
    
    
    public override void OnActionReceived(ActionBuffers actions) {
        // opn le recompense si il fait une action
        AddReward(0.2f);
        
        if (actions.DiscreteActions[0] == 1) {
            bird.Jump();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint")) {
            Debug.Log("Checkpoint AI");
            AddReward(1f);
        } else {
            
            Debug.Log("Collision AI");
            SetReward(-1f);
            EndEpisode();
            
        }
    }
    
    
    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = isJumpInputDown ? 1 : 0;

        isJumpInputDown = false;
    }
}
