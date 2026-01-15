using UnityEngine;

public class ObstacleDetectionSensor : MonoBehaviour
{
    public AIController.SensorPosition sensorPosition;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(!Application.isPlaying)
        {
            gameObject.name = $"Sensor[{sensorPosition}]";
        }
    }
#endif
}
