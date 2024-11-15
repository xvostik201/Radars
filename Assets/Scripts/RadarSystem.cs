using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadarSystem : MonoBehaviour
{
    [Header("Настройки радара")]
    [SerializeField] private RectTransform _radarBox;  
    [SerializeField] private RectTransform _radarUI;
    [SerializeField] private RectTransform _radarSweep;
    [SerializeField] private Transform _radarOrigin;   
    [SerializeField] private float _lineError = 5f;    
    [SerializeField] private float _radarRange = 100f; 
    [SerializeField] private float _rotationSpeed = 30f;
    [SerializeField] private float _radarHeight = 10f; 

    [Header("Настройки blip'ов")]
    [SerializeField] private GameObject _blipPrefab;
    [SerializeField] private float _blipLifetime = 1f;
    [SerializeField] private float _sizeFactor = 1.0f;
    [SerializeField] private float _minBlipSize = 5f;
    [SerializeField] private float _maxBlipSize = 20f;

    [Header("Текст")]
    [SerializeField] private GameObject _textPrefab;
    [SerializeField] private float _textYError;

    private float currentAngle = 0f;

    private HashSet<Collider> _trackedColliders = new HashSet<Collider>();

    private void Start()
    {
        AdjustRadarSize();
        AdjustRadarLine();
    }

    void Update()
    {
        currentAngle += _rotationSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        if (_radarSweep != null)
        {
            _radarSweep.localEulerAngles = new Vector3(0, 0, -currentAngle);
        }

        Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
        Debug.DrawRay(_radarOrigin.position, direction * _radarRange, Color.green);

        Vector3 halfExtents = new Vector3(0.5f, _radarHeight / 2, 0.5f);
        Vector3 origin = _radarOrigin.position + Vector3.up * (_radarHeight / 2);

        RaycastHit hit;
        if (Physics.BoxCast(origin, halfExtents, direction, out hit, Quaternion.identity, _radarRange))
        {
            if (!_trackedColliders.Contains(hit.collider))
            {
                Vector3 hitPosition = hit.point;

                Vector2 objectSize = GetObjectSize(hit.collider);

                Vector2 radarPos = WorldToRadarPosition(hitPosition);

                float distance = Vector3.Distance(hitPosition, transform.position);

                CreateBlip(radarPos, objectSize, distance, hit.transform.position.y);

                Debug.DrawLine(_radarOrigin.position, hitPosition, Color.red);

                _trackedColliders.Add(hit.collider);
            }
        }
        _trackedColliders.RemoveWhere(collider => !hit.collider);
    }

    private Vector2 GetObjectSize(Collider collider)
    {
        Bounds bounds = collider.bounds;
        Vector2 size = new Vector2(bounds.size.x, bounds.size.z);
        return size;
    }

    Vector2 WorldToRadarPosition(Vector3 worldPosition)
    {
        Vector3 delta = worldPosition - _radarOrigin.position;

        float dx = delta.x;
        float dz = delta.z;

        float normalizedX = dx / _radarRange;
        float normalizedY = dz / _radarRange;

        float radarWidth = _radarUI.rect.width;
        float radarHeightUI = _radarUI.rect.height;

        Vector2 radarPos = new Vector2(normalizedX * (radarWidth / 2), normalizedY * (radarHeightUI / 2));

        return radarPos;
    }

    float WorldToRadarSweepUI()
    {
        float height = (_radarUI.sizeDelta.x / 2) - (_radarUI.sizeDelta.x / _lineError);
        return height;
    }

    Vector2 WorldToRadarUI()
    {
        return new Vector2(_radarBox.rect.width, _radarBox.rect.height);
    }

    private void AdjustRadarSize()
    {
        _radarUI.sizeDelta = new Vector2(_radarBox.rect.width, _radarBox.rect.height);
    }

    private void AdjustRadarLine()
    {
        _radarSweep.sizeDelta = new Vector2(1, WorldToRadarSweepUI());
    }

    private void CreateBlip(Vector2 pos, Vector2 size, float distance, float altitude)
    {
        GameObject text = Instantiate(_textPrefab, _radarUI);
        GameObject blip = Instantiate(_blipPrefab, _radarUI);

        RectTransform bleepRectTransform = blip.GetComponent<RectTransform>();
        RectTransform textRectTransform = text.GetComponent<RectTransform>();

        Vector2 textPos = new Vector2(pos.x, pos.y / _textYError);

        TMP_Text TMP_text = text.GetComponent<TMP_Text>();

        TMP_text.text = $"d - {Mathf.Round(distance)}, a - {MathF.Round(altitude)}";

        textRectTransform.anchoredPosition = textPos;
        bleepRectTransform.anchoredPosition = pos;

        Vector2 bleepSize = size * _sizeFactor;

        bleepSize.x = Mathf.Clamp(bleepSize.x, _minBlipSize, _maxBlipSize);
        bleepSize.y = Mathf.Clamp(bleepSize.y, _minBlipSize, _maxBlipSize);

        bleepRectTransform.sizeDelta = bleepSize;
        Debug.Log(bleepSize);
        Destroy(blip, _blipLifetime);
        Destroy(text, _blipLifetime);
    }
}
