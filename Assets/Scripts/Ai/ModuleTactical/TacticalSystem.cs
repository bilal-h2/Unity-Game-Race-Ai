using System.Collections.Generic;
using UnityEngine;

public class TacticalSystem : MonoBehaviour
{
    public enum WeatherCondition
    {
        Clear,
        Rain,
        Fog,
        Snow
    }
    public class OpponentData
    {
        public string Name { get; set; }
        public float Distance { get; set; }
        public float Speed { get; set; }
        public int TeamID { get; set; }
        public int RacerID { get; set; }

        public bool IsAhead(VehicleController aiVehicle)
        {
            return Distance < aiVehicle.DistanceTraveled;
        }

        public bool IsSlower(VehicleController aiVehicle)
        {
            return Speed < aiVehicle.GetCurrentSpeed;
        }
    }
    private AIController vehicleAiController;
    private VehicleController vehicleController;

    private void Start()
    {
        vehicleAiController = GetComponent<AIController>();
    }

    public void MakeTacticalDecisions()
    {
        HandleObstacleAvoidance();
        HandleWeatherAdaptation();
        HandleOpponentBehaviorAnalysis();
        HandleDrafting();
        HandleCorneringStrategy();
        HandleTeamCollaboration();
    }
    private void HandleWeatherAdaptation()
    {
        //Create a weather manager and get the current weather situation by position of the vehicle.
        WeatherCondition currentWeather = WeatherCondition.Clear;

        switch (currentWeather)
        {
            case WeatherCondition.Clear:
                break;
            case WeatherCondition.Rain:
                AdaptToRainyWeather();
                break;
            case WeatherCondition.Fog:
                AdaptToFoggyWeather();
                break;
            case WeatherCondition.Snow:
                AdaptToSnowyWeather();
                break;
        }
    }

    private void AdaptToRainyWeather()
    {
        vehicleAiController.MaximumSpeed *= 0.8f;

        vehicleAiController.BrakeForce *= 1.2f;

        vehicleAiController.SteeringSensitivity *= 0.7f;
    }

    private void AdaptToFoggyWeather()
    {
        vehicleAiController.MaximumSpeed *= 0.7f;

        vehicleAiController.BrakeForce *= 1.1f;
    }

    private void AdaptToSnowyWeather()
    {
        vehicleAiController.MaximumSpeed *= 0.6f;

        vehicleAiController.BrakeForce *= 1.3f;

        vehicleAiController.FollowingDistance *= 1.5f;
    }


    public void HandleObstacleAvoidance()
    {
        OpponentData closestOpponent = GetClosestOpponent();

        if (closestOpponent != null)
        {
            float safeDistance = CalculateSafeDistance();

            //bool clearPathForOvertake = obstacleAvoidance.CheckClearPathForOvertake(closestOpponent.Distance, safeDistance);

            //if (!clearPathForOvertake)
            //{
            //    ApplyDefensiveBehavior();
            //}
        }
    }

    private OpponentData GetClosestOpponent()
    {
        OpponentData[] opponentsData = OpponentManager.Instance.GetOpponentData;
        OpponentData closestOpponent = null;
        float closestDistance = float.MaxValue;

        foreach (OpponentData opponentData in opponentsData)
        {
            float distanceToOpponent = opponentData.Distance - vehicleController.DistanceTraveled;

            if (distanceToOpponent > 0 && distanceToOpponent < closestDistance)
            {
                closestOpponent = opponentData;
                closestDistance = distanceToOpponent;
            }
        }

        return closestOpponent;
    }

    private float CalculateSafeDistance()
    {
        return vehicleController.GetCurrentSpeed * 1.5f;
    }

    private void ApplyDefensiveBehavior()
    {

    }

    private void HandleOpponentBehaviorAnalysis()
    {
        OpponentData[] opponents = OpponentManager.Instance.GetOpponentData;

        foreach (OpponentData opponent in opponents)
        {
            if (opponent.IsAhead(vehicleController))
            {
                if (opponent.IsSlower(vehicleController))
                {
                    if (ShouldOvertakeOpponent(opponent))
                    {

                    }
                }
                else
                {
                    if (ShouldDefendAgainstOpponent(opponent))
                    {

                    }
                }
            }
        }
    }

    private bool ShouldOvertakeOpponent(OpponentData opponent)
    {
        float safeDistanceToOvertake = vehicleController.GetCurrentSpeed * 2.5f;

        bool isSignificantlyFaster = vehicleController.GetCurrentSpeed > opponent.Speed * 1.2f;
        //bool hasClearPath = obstacleAvoidance.CheckClearPathForOvertake(opponent.Distance, safeDistanceToOvertake);

        return isSignificantlyFaster;// && hasClearPath;
    }

    private bool ShouldDefendAgainstOpponent(OpponentData opponent)
    {
        float defensiveDistance = vehicleController.GetCurrentSpeed * 1.5f;

        bool isSignificantlyFaster = opponent.Speed > vehicleController.GetCurrentSpeed * 1.2f;
        bool withinDefensiveDistance = opponent.Distance < defensiveDistance;

        //bool hasClearPath = obstacleAvoidance.CheckClearPathForDefense(defensiveDistance);

        return isSignificantlyFaster && withinDefensiveDistance;// && hasClearPath;
    }

    private void HandleDrafting()
    {
        OpponentData closestOpponent = GetClosestOpponent();

        if (closestOpponent != null)
        {
            float relativeSpeedDifference = vehicleController.GetCurrentSpeed - closestOpponent.Speed;

            if (relativeSpeedDifference > 0 && relativeSpeedDifference < 10f)
            {
                float targetSpeed = closestOpponent.Speed;
                AdjustSpeedTowards(targetSpeed);
            }
        }
    }
    private void AdjustSpeedTowards(float targetSpeed)
    {

        //set the input to vehicle controller
    }
    private void HandleCorneringStrategy()
    {
        float currentSteeringAngle = vehicleController.GetCurrentSteerAngle;
        Vector3 currentPosition = transform.position;

    }

    private void HandleTeamCollaboration()
    {
        List<OpponentData> teammates = OpponentManager.Instance.GetTeammates(vehicleController.RacerID, vehicleController.TeamID);

        foreach (OpponentData teammate in teammates)
        {
            float relativeDistance = teammate.Distance - vehicleController.DistanceTraveled;

            if (relativeDistance < 50f)
            {
                AdjustSpeedToMaintainDistance(relativeDistance, 40f);
            }
        }
    }
    private void AdjustSpeedToMaintainDistance(float relativeDistance, float targetDistance)
    {
        float currentSpeed = vehicleController.GetCurrentSpeed;


    }
}
