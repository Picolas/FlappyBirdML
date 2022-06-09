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
    public event  EventHandler OnStartedPlaying;

    private Rigidbody2D rigidbody2D;

    private const float JUMP_AMOUNT = 100f;
    private State state;

    private enum State {
        WaitingToStart,
        Playing,
        Dead,
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        instance = this;
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Static;
        state = State.WaitingToStart;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state){
        default: 
        case State.WaitingToStart:
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                state = State.Playing;
                rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                Jump();
                if(OnStartedPlaying != null) OnStartedPlaying(this, EventArgs.Empty);
            }
            break;
        case State.Playing:
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                Jump();
            }
            
            transform.eulerAngles = new Vector3(0, 0, rigidbody2D.velocity.y * 0.2f);
            break;
        case State.Dead:
            break;
        }
    }

    void Jump()
    {
        rigidbody2D.velocity = Vector2.up * JUMP_AMOUNT;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision");
        rigidbody2D.bodyType = RigidbodyType2D.Static;
        if(OnDied != null ) OnDied(this, EventArgs.Empty);
    }
}
