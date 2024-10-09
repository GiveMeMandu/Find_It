using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Cart cart;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Run() {
        cart.StartWheel();
    }
    public void Stop() {
        cart.StopWheel();
    }
    
}
