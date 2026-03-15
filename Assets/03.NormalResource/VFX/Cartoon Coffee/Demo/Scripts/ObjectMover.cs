using UnityEngine;

namespace Demo_Project
{
    public class ObjectMover : MonoBehaviour
    {
        // Set the desired position for the object
        public Vector3 targetPosition = new Vector3(0, 0, 0);

        // Update is called once per frame
        void Start()
        {
            // Check for user input to move the object
            MoveObject();
        }

        void MoveObject()
        {
            // Set the object's position to the target position
            transform.position = targetPosition;
        }
    }
}
