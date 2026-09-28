using System;
using UnityEngine;

public class Health : IValueble
{
    protected float _maxValue;
    protected float _value;

    public float MaxValue { get => _maxValue; }
    public float Value { get => _value; }

    public Action<float> ValueChanged { get; set; }
    public Action<float> MaxValueChanged { get; set; }

    public Action Ended;

    public void Init(float maxValue, float currentValue = 0)
    {
        _maxValue = maxValue;
        _value = currentValue;

        MaxValueChanged?.Invoke(maxValue);
        ValueChanged?.Invoke(maxValue);
    }

    public void Take(float value)
    {
        _value -= value;

        if(_value <= 0)
        {
            _value = 0;
            Ended?.Invoke();
        }
        ValueChanged?.Invoke(_value);
    }

    public virtual void Add(float value)
    {
        _value += value;

        if(_value > _maxValue)
            _value = _maxValue;

        ValueChanged?.Invoke(_value);
    }
}