using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SimpleRacePositionSystem : MonoBehaviour
{
    private Dictionary<AIController, float> participantProgress = new Dictionary<AIController, float>();
    
    List<AIController> sortedParticipants = new List<AIController>();

    public void AddParticipant(AIController controller)
    {
        participantProgress.Add(controller, 0f);
    }
    public void SyncProgress(AIController _participant)
    {
        float _progress = _participant.Lap * _participant.waypointSystem.waypoints.Length;
        _progress = (_progress + _participant.WaypointsPassed) * 10f;

        participantProgress[_participant] = _progress;
    }

    public int GetPosition(AIController _participant)
    {
        int position = 1;
        float participantProgressValue = participantProgress[_participant];

        foreach (KeyValuePair<AIController, float> kvp in participantProgress)
        {
            if (kvp.Value > participantProgressValue)
            {
                position++;
            }
        }

        return position;
    }
}
