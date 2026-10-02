using AI.InfluenceMap;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUiManager : MonoBehaviour
{
    InfluenceMap influenceMap;

    ETeam currentTeam;
    public TMP_Dropdown dropdown;
    public Button teamSwitchButton;
    public Image buttonImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        influenceMap = FindFirstObjectByType<InfluenceMap>();
        currentTeam = influenceMap.controller.GetTeam();
        teamSwitchButton.onClick.AddListener(() => {
            ETeam newTeam = currentTeam == ETeam.Red ? ETeam.Blue : ETeam.Red;
            TeamSwitch(newTeam);
        });
        SetupForTeam(currentTeam);
        switch (currentTeam)
        {
            case ETeam.Red:
                buttonImage.color = Color.red;
                break;
            case ETeam.Blue:
                buttonImage.color = Color.blue;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(dropdown.value == 0)//None
        {
            influenceMap.showDebug = false;
        }
        else if (dropdown.value == 1)//All
        {
            influenceMap.showDebug = true;
            influenceMap.mapToShow = -1;
        }
        else
        {
            influenceMap.mapToShow = dropdown.value - 2;
        }
    }

    void SetupForTeam(ETeam team)
    {
        dropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData("None"));
        options.Add(new TMP_Dropdown.OptionData("All"));
        foreach (var subMap in influenceMap.subMapList)
        {
            options.Add(new TMP_Dropdown.OptionData(subMap.ToString()));
        }
        dropdown.AddOptions(options);
    }
    void TeamSwitch(ETeam team)
    {
        List<UnitController> controllers = new List<UnitController>(FindObjectsByType<UnitController>(FindObjectsSortMode.None));
        foreach (var controller in controllers)
        {
            if (controller.GetTeam() == team)
            {
                influenceMap.ChangeController(controller);
            }
        }
        switch(team)
        {
            case ETeam.Red:
                currentTeam = ETeam.Red;
                buttonImage.color = Color.red;
                break;
            case ETeam.Blue:
                currentTeam = ETeam.Blue;
                buttonImage.color = Color.blue;
                break;
        }
    }
}


