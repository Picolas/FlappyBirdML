using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    private static Bird instance;

    public static Bird GetInstance(){
        return instance;
    }
    
    public event  EventHandler OnDied;
    public event  EventHandler OnStartPlay;

    private Rigidbody2D rigidbody2D;

    private const float JUMP_AMOUNT = 100f;
    private State state;

    private enum State {
        WaitStart,
        Playing,
        GameOver,
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public float GetVelocityY() {
        return rigidbody2D.velocity.y;
    }

    void Awake()
    {
        instance = this;
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Static;
        state = State.WaitStart;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state){
        default: 
        case State.WaitStart:
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                state = State.Playing;
                rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                Jump();
                if(OnStartPlay != null) OnStartPlay(this, EventArgs.Empty);
            }
            break;
        case State.Playing:
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                Jump();
            }
            
            transform.eulerAngles = new Vector3(0, 0, rigidbody2D.velocity.y * 0.2f);
            break;
        case State.GameOver:
            break;
        }
    }

    public void Jump()
    {
        rigidbody2D.velocity = Vector2.up * JUMP_AMOUNT;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint")) {
            Debug.Log("Checkpoint");
        } else {
            
            Debug.Log("Collision");
            rigidbody2D.bodyType = RigidbodyType2D.Static;
            if(OnDied != null ) OnDied(this, EventArgs.Empty);
            
        }
    }
    
    public void Reset() {
        // On reset tout le bird avec le reset de ses coordonnées, movement etc
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.bodyType = RigidbodyType2D.Static;
        transform.position = Vector3.zero;
        state = State.WaitStart;
    }
}
