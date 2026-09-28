using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ApplyDamageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (hp, damageThisFrame) in SystemAPI.Query<RefRW<HealthComponentData>, DynamicBuffer<DamageOnThisFrameBuffer>>())
        {
            if (damageThisFrame.IsEmpty)
                continue;

            foreach (var damage in damageThisFrame)
            {
                hp.ValueRW.Value -= damage.Value;
            }
            damageThisFrame.Clear();
        }
    }
}
