using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GamePlayManager : MonoBehaviour
{
    [Serializable]
    public struct AiVehicleSpawnHandler
    {
        public Transform aiSpawnPoint;
        public Color AiColor;
    }

    public Transform PlayerSpawnPoint;
    public AiVehicleSpawnHandler[] AiSpawnPoints;

    [Space(5)]
    public GameObject PlayerVehiclePrefab;
    public GameObject[] AIVehiclePrefabs;
    
    [Space(5)]
    public Button ChangeCameraButton;

    private GameObject _PlayerVehicleInstance;
    private GameObject[] _AiInstances;

    private FollowCam followCam;
    private bool FollowingPlayer = false;

    private int AiTarget = 0;

    private Transform GetNextAiTarget
    {
        get
        {
            Transform _target = _AiInstances[AiTarget].transform;
            AiTarget = (AiTarget + 1) % _AiInstances.Length;

            return _target;
        }
    }

    public void SimulationSpeed(float speed)
    {
        Time.timeScale = speed;
    }
    void Start()
    {
        if (Time.timeScale < 1)
        {
            Time.timeScale = 1;
        }

        followCam = FindObjectOfType<FollowCam>();
        if(ChangeCameraButton)
            ChangeCameraButton.onClick.AddListener(ChangeCamera);
       
        SpawnPlayer();
        StartCoroutine(SpawnAis());
    }
    private void OnDisable()
    {
        Time.timeScale = 1;
    }
    private void SpawnPlayer()
    {
        if (PlayerSpawnPoint == null) return;

        _PlayerVehicleInstance = Instantiate(PlayerVehiclePrefab, PlayerSpawnPoint.position, PlayerSpawnPoint.rotation);
        

        followCam.SetTarget(_PlayerVehicleInstance.transform);
        FollowingPlayer = true;
    }
    private IEnumerator SpawnAis()
    {
        List<GameObject> _aiVehiclesList = new List<GameObject>();

        foreach (var point in AiSpawnPoints)
        {
            int randomIndex = Random.Range(0, AIVehiclePrefabs.Length);
            _aiVehiclesList.Add
                (
                Instantiate(AIVehiclePrefabs[randomIndex], point.aiSpawnPoint.position, point.aiSpawnPoint.rotation)
                );

            _aiVehiclesList[_aiVehiclesList.Count - 1].name = "_AI_" + point.aiSpawnPoint.name;

            if (_aiVehiclesList[_aiVehiclesList.Count - 1].GetComponent<NavMeshAgent>())
            {
                _aiVehiclesList[_aiVehiclesList.Count - 1].GetComponent<AIController>().StatsColor = point.AiColor;
                _aiVehiclesList[_aiVehiclesList.Count - 1].transform.Find("AiColor").GetComponent<MeshRenderer>().material.color = point.AiColor;
            }
            yield return new WaitForSeconds(0.25f);
        }

        _AiInstances = _aiVehiclesList.ToArray();

        if(_PlayerVehicleInstance == null)
        {
            ChangeCamera();
        }
    }
    public void ChangeCamera()
    {
        if(FollowingPlayer || _PlayerVehicleInstance == null)
        {
            followCam.SetTarget(GetNextAiTarget);
            FollowingPlayer = false;
        }
        else if(_PlayerVehicleInstance != null)
        {
            followCam.SetTarget(_PlayerVehicleInstance.transform);
            FollowingPlayer = true;
        }
    }
    public void Pause(bool isPause)
    {
        if (isPause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ResetPlayerVehicle()
    {
        var temp = _PlayerVehicleInstance;
        temp.SetActive(false);

        SpawnPlayer();

        Destroy(temp);
    }
}
