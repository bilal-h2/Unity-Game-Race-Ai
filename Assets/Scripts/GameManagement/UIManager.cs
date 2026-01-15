using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform uiContainer;
    public GameObject _uiPrefab;

    private List<UIManagerElement> uIManagerElements= new List<UIManagerElement>();

    private Dictionary<AIController, int> indexes = new Dictionary<AIController, int>();
    
    public void SetUI(AIController ai)
    {
        // Create a new UI element prefab here
        UIManagerElement uiElement = Instantiate(_uiPrefab, uiContainer).GetComponent<UIManagerElement>();

        uIManagerElements.Add(uiElement);

        indexes.Add(ai, uIManagerElements.Count-1);
        
        uiElement.SetID(ai);
    }

    public void UpdateUI(AIController ai)
    {
        int index = indexes[ai];

        uIManagerElements[index].SetUI(ai);
    }
}

