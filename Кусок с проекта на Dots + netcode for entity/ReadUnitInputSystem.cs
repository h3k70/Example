using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct ReadUnitInputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var netTime = SystemAPI.GetSingleton<NetworkTime>();

        var targetDestinationLookup = SystemAPI.GetComponentLookup<TargetDestinationComponent>(false);
        var SkillbufferLookup = SystemAPI.GetBufferLookup<SkillBuffer>(false);
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        foreach (var (input, entity) in SystemAPI.Query<RefRO<UnitInputComponent>>().WithAll<Simulate>().WithEntityAccess())
        {
            if (netTime.IsFirstTimeFullyPredictingTick)
            {
                if (input.ValueRO.ActionSet.IsSet)
                {
                    switch (input.ValueRO.Command)
                    {
                        case UnitCommands.Move:
                            if (targetDestinationLookup.HasComponent(entity))
                            {
                                ecb.SetComponent(entity, new TargetDestinationComponent
                                {
                                    Value = input.ValueRO.TargetPosition
                                });
                            }
                            break;
                        case UnitCommands.UseAbility:
                            if (SkillbufferLookup.HasBuffer(entity))
                            {
                                var buffer = SkillbufferLookup[entity];
                                var tempSkill = buffer[input.ValueRO.AbilityNum];

                                if (tempSkill.CurrentCooldown > 0)
                                    continue;

                                tempSkill.Target = input.ValueRO.TargetEntity;
                                tempSkill.Vector = input.ValueRO.TargetPosition;
                                tempSkill.CurrentCastTime = tempSkill.CastTime;
                                tempSkill.CurrentCooldown = tempSkill.MaxCooldown;
                                tempSkill.Status = SkillStatus.Casting;

                                buffer[input.ValueRO.AbilityNum] = tempSkill;                      
                            }
                            break;
                    }
                }
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}