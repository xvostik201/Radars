using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SonarSystem : MonoBehaviour
{
    [Header("Sonar Settings")]
    [SerializeField] private float _sonarRadius;
    [SerializeField] private float _sonarSpeed;

    [Header("UI")]
    [SerializeField] private RectTransform _radarBox;
    [SerializeField] private RectTransform _radarUI;
    [SerializeField] private RectTransform _sonarUI;
    [SerializeField] private Transform _sonarOrigin;

    [Header("Blip Settings")]
    [SerializeField] private GameObject _blipPrefab;
    [SerializeField] private float _blipLifetime;
    [SerializeField] private float _blipSizeFactor;
    [SerializeField] private float _minBlipSize;
    [SerializeField] private float _maxBlipSize;

    [Header("Текст")]
    [SerializeField] private GameObject _textPrefab;
    [SerializeField] private float _textYError;

    private float _currentSonarRadius;
    private bool _isExpanding = true;

    private HashSet<Collider> _trackedColliders = new HashSet<Collider>();

    private void Start()
    {
        _radarUI.sizeDelta = WorldToRadarUI();
    }

    void Update()
    {
        if (_currentSonarRadius >= _sonarRadius)
        {
            _sonarSpeed = -Mathf.Abs(_sonarSpeed); 
            _isExpanding = false;
        }
        else if (_currentSonarRadius <= 0f)
        {
            _sonarSpeed = Mathf.Abs(_sonarSpeed);
            _isExpanding = true;
        }

        _currentSonarRadius = Mathf.Clamp(_currentSonarRadius + _sonarSpeed * Time.deltaTime, 0, _sonarRadius);

        _sonarUI.sizeDelta = new Vector2(WorldToUIRadius(), WorldToUIRadius());

        Collider[] hitColliders = Physics.OverlapSphere(_sonarOrigin.position, _currentSonarRadius);

        foreach (Collider collider in hitColliders)
        {
            if (_isExpanding)
            {
                if (!_trackedColliders.Contains(collider))
                {
                    Vector3 hitPosition = collider.transform.position;
                    Vector2 objectSize = GetObjectSize(collider);
                    Vector2 radarPos = WorldToRadarPosition(hitPosition);
                    float distance = Vector3.Distance(hitPosition, transform.position);
                    CreateBlip(radarPos, objectSize, distance);

                    _trackedColliders.Add(collider);
                }
            }
        }

        if (!_isExpanding)
        {
            _trackedColliders.RemoveWhere(collider =>
            {
                bool isStillInRadius = Array.Exists(hitColliders, hit => hit == collider);
                if (!isStillInRadius)
                {
                    Vector3 exitPosition = collider.transform.position;
                    Vector2 objectSize = GetObjectSize(collider);
                    Vector2 radarPos = WorldToRadarPosition(exitPosition);
                    float distance = Vector3.Distance(exitPosition, transform.position);
                    CreateBlip(radarPos, objectSize, distance);
                }
                return !isStillInRadius; 
            });
        }
    }

    Vector2 WorldToRadarPosition(Vector3 worldPosition)
    {
        Vector3 delta = worldPosition - _sonarOrigin.position;

        float deltax = delta.x;
        float deltaz = delta.z;

        float normalizedx = deltax / _sonarRadius;
        float normalizedy = deltaz / _sonarRadius;

        float radarWidth = _radarUI.rect.width;
        float radarHeight = _radarUI.rect.height;

        Vector2 radarPos = new Vector2(normalizedx * (radarWidth / 2), normalizedy * (radarHeight / 2));

        return radarPos;
    }

    float WorldToUIRadius()
    {
        float delta = (_radarUI.sizeDelta.y / _sonarRadius) * _currentSonarRadius;
        return delta;
    }

    Vector2 WorldToRadarUI()
    {
        return new Vector2(_radarBox.rect.width, _radarBox.rect.height);
    }

    private Vector2 GetObjectSize(Collider collider)
    {
        Bounds bounds = collider.bounds;
        Vector2 size = new Vector2(bounds.size.x, bounds.size.z);
        return size;
    }

    private void CreateBlip(Vector2 pos, Vector2 size, float distance)
    {
        GameObject text = Instantiate(_textPrefab, _radarUI);
        GameObject blip = Instantiate(_blipPrefab, _radarUI);

        RectTransform bleepRectTransform = blip.GetComponent<RectTransform>();
        RectTransform textRectTransform = text.GetComponent<RectTransform>();

        Vector2 textPos = new Vector2(pos.x, pos.y / _textYError);

        TMP_Text TMP_text = text.GetComponent<TMP_Text>();

        TMP_text.text = $"d - {MathF.Round(distance)}";

        textRectTransform.anchoredPosition = textPos;
        bleepRectTransform.anchoredPosition = pos;

        Vector2 bleepSize = size * _blipSizeFactor;

        bleepSize.x = Mathf.Clamp(bleepSize.x, _minBlipSize, _maxBlipSize);
        bleepSize.y = Mathf.Clamp(bleepSize.y, _minBlipSize, _maxBlipSize);

        bleepRectTransform.sizeDelta = bleepSize;
        Debug.Log(bleepSize);
        Destroy(blip, _blipLifetime);
        Destroy(text, _blipLifetime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawSphere(_sonarOrigin.position, _currentSonarRadius);
    }
}
