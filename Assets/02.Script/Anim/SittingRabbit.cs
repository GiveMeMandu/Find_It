using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SittingRabbit : MonoBehaviour
{
    public enum State
    {
        Walk,
    }
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAnim(string name) { 
        animator.Play(name);
    }
    public void SetAnim(State state) { 
        animator.Play(state.ToString());
    }
}
