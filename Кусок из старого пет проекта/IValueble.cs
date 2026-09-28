using System;

public interface IValueble
{
    public float MaxValue { get; }
    public float Value { get; }

    public Action<float> ValueChanged { get; set; }
    public Action<float> MaxValueChanged { get; set; }
}
