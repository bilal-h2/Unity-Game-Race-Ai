using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TacticalSystem;

public class OpponentManager : MonoBehaviour
{
    private List<OpponentData> opponentsData = new List<OpponentData>();

    private static OpponentManager instance;
    public static OpponentManager Instance
    {
        get
        {
            if(!instance) instance = FindObjectOfType<OpponentManager>();

            return instance;
        }
    }

    //call this function from race initializer
    //add all the vehicles in this list including player
    public void AddOpponentData(OpponentData opponentData)
    {
        opponentsData.Add(opponentData);
    }

    public OpponentData[] GetOpponentData
    {
        get
        {
            return opponentsData.ToArray();
        }
    }
    public List<OpponentData> GetTeammates(int RacerID, int teamID)
    {
        List<OpponentData> teammates = new List<OpponentData>();

        foreach (OpponentData opponent in opponentsData)
        {
            if (opponent.TeamID == teamID)
            {
                teammates.Add(opponent);
            }
        }

        return teammates;
    }
}

