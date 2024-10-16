using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Butterfly : MonoBehaviour
{   
    public float maxTimer = 5f;
    public float timer = 0f;
    public float moveSpeed = 1f;
    public float flyForce = 1f;
    public GameObject visualObj;
    public List<Transform> goalList;

    const float FLY_FORCE = 1.45f;
    const float GRAVITY = 0.25f;
    const float MIN_DIST = 0.1f;
    Animator _animator;
    Rigidbody2D _rigid;
    
    enum State { Fly, Sit, Hold }
    State _currentState;
    Vector3 _goalPosition = Vector3.zero;
    private int goalCount = 0;
    void Start()
    {
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody2D>();
        _rigid.gravityScale = GRAVITY * flyForce;
        _goalPosition = goalList[goalCount].position;
        goalCount++;
        ChangeState(State.Fly);
    }

    // Update is called once per frame
    void Update()
    {
        if(_currentState == State.Fly) {
            if(Vector3.Distance(_goalPosition, transform.position) <= MIN_DIST) {
                ChangeState(State.Sit);
                return;
            }
            Vector3 direction = (_goalPosition - transform.position).normalized;
            transform.position += moveSpeed * Time.deltaTime * direction;
            FlipX();
        }
        else if(_currentState == State.Hold) {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }else if(_currentState == State.Sit)
        {
            timer += Time.deltaTime;
            if(timer > maxTimer)
            {
                timer = 0;
                _goalPosition = goalList[goalCount].position;
                goalCount++;
                if(goalCount > goalList.Count - 1) goalCount = 0;
                ChangeState(State.Fly);
            }
        }
    }
    public void AddToGoalList(Transform goal) {
        goalList.Add(goal);
    }

    void FlipX() {
        if(_goalPosition.x > visualObj.transform.position.x) {
            visualObj.transform.localScale = new Vector3(1,1,1);
        }
        else if(_goalPosition.x < visualObj.transform.position.x) {
            visualObj.transform.localScale = new Vector3(-1,1,1);
        }
    }

    void ChangeState(State nextState) {
        _currentState = nextState;
        switch (_currentState)
        {
            case State.Fly:
                // _goalPosition = goalList.OrderBy(goal => Vector3.Distance(goal.position, transform.position)).First().position;
                _rigid.isKinematic = false;
                break;
            default:
                _rigid.isKinematic = true;
                _rigid.velocity = Vector2.zero;
                _rigid.totalForce = Vector2.zero;
                break;
        }
        _animator.Play(_currentState.ToString());
    }

    void Force() {
        _rigid.velocity = Vector2.zero;
        _rigid.AddForce(FLY_FORCE * flyForce * new Vector2(0,1), ForceMode2D.Impulse);
    }
    public void Hold(bool isHold = true) {
        if(isHold) 
            ChangeState(State.Hold);
        else
            ChangeState(State.Fly);
    }
    
}
