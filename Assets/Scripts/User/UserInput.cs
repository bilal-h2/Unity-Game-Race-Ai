using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    //Keeping a reference to the main vehicle controller for sending inputs
    private VehicleController vehicleController;

    private void Start()
    {
        //Getting the reference on start or the game
        vehicleController = GetComponent<VehicleController>();
    }

    private void Update()
    {
        //Getting the accel input
        float accelerationInput = Input.GetAxis("Vertical");
        //Getting the steering input
        float steeringInput = Input.GetAxis("Horizontal");
        //Getting the braking input
        float braking = Input.GetKey(KeyCode.Space) ? 1 : 0;


        //Sending the inputs to vehicle
        vehicleController.SetInput(accelerationInput, steeringInput, braking);
    }
}