using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(ApplyDamageSystem))]
partial struct DestroyEntitySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entityCommandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (destroyFlag, entity) in SystemAPI.Query<DestroyFlag>().WithEntityAccess())
        {
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }
}
