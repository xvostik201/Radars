using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchSystem : MonoBehaviour
{
    [Header("Radar")]
    [SerializeField] private RadarSystem _radarSystem;
    [SerializeField] private GameObject[] _radarObjects;

    [Header("Sonar")]
    [SerializeField] private SonarSystem _sonarSystem;
    [SerializeField] private GameObject[] _sonarObjects;

    [Header("UI")]
    [SerializeField] private Button _radarActivate;
    [SerializeField] private Button _sonarActivate;
    void Start()
    {
        _radarActivate.onClick.AddListener(() => ActiveSystem(true));
        _sonarActivate.onClick.AddListener(() => ActiveSystem(false));

        ActiveSystem(true);
    }

    private void ActiveSystem(bool isRadarSystem)
    {
        if (isRadarSystem)
        {
            _radarSystem.enabled = true;
            _sonarSystem.enabled = false;
            foreach (var obj in _radarObjects)
            {
                obj.SetActive(true);
            }
            foreach (var obj in _sonarObjects)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            _radarSystem.enabled = false;
            _sonarSystem.enabled = true;
            foreach (var obj in _radarObjects)
            {
                obj.SetActive(false);
            }
            foreach (var obj in _sonarObjects)
            {
                obj.SetActive(true);
            }
        }
    }
}
