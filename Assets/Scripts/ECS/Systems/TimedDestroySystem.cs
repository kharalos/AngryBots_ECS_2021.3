using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(MoveForwardSystem))]
[AlwaysSynchronizeSystem]
public partial class TimedDestroySystem : SystemBase
{
  protected override void OnUpdate()
  {
    var deltaTime = Time.DeltaTime;
    var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

    Entities
      .WithBurst()
      .ForEach((Entity entity, ref TimeToLive timeToLive) =>
      {
        timeToLive.Value -= deltaTime;
        if (timeToLive.Value <= 0f)
          commandBuffer.DestroyEntity(entity);
      }).Run();

    commandBuffer.Playback(EntityManager);
    commandBuffer.Dispose();
  }
}

