using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedMeterUI : MonoBehaviour
{
    public Text SpeedText;

    public void SetUI(float speed, int CurrentGear)
    {
        SpeedText.text = "SPEED : " + speed.ToString("#.##") + " KMPh";
        SpeedText.text += "\nGear : " + CurrentGear;
    }
}
