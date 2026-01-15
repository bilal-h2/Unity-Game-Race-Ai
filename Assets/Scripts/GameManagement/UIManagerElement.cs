using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerElement : MonoBehaviour
{
    [SerializeField] private Image AiColorBorder;
    [Header("Texts")]
    [SerializeField] private Text Speed;
    [SerializeField] private Text Gear;
    [SerializeField] private Text Steering;
    [SerializeField] private Text EngineRPM;
    [SerializeField] private Text Decision;

    public void SetID(AIController ai)
    {
        AiColorBorder.color = ai.StatsColor;
    }
    public void SetUI(AIController ai)
    {
        Speed.text = "SPEED:" + ai.vehicleController.GetCurrentSpeed.ToString();
        Gear.text = "GEAR:" + ai.vehicleController.GetCurrentGear.ToString();
        Steering.text = "STEER:" + (ai.vehicleController.GetCurrentSteerAngle < 0 ? "Left" : "Right");
        Decision.text = ai.GetActiveDecisions();
    }

}
