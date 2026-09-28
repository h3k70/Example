using System;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour, IDisposable
{
    [SerializeField] private Slider _slider;

    private IValueble _resource;
    private float _maxValue;

    public void Init(IValueble resource)
    {
        _resource = resource;
        _maxValue = _resource.MaxValue;

        OnValueChanged(_resource.Value);

        _resource.MaxValueChanged += OnMaxValueChanged;
        _resource.ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(float value)
    {
        _slider.value = value / _maxValue;
    }

    private void OnMaxValueChanged(float value)
    {
        _maxValue = value;
    }

    public void Dispose()
    {
        _resource.MaxValueChanged -= OnMaxValueChanged;
        _resource.ValueChanged -= OnValueChanged;
    }
}
