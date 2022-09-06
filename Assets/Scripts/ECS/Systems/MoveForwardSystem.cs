using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Transforms {
  public partial class MoveForwardSystem : SystemBase
  {
    protected override void OnUpdate()
    {
      var dt = Time.DeltaTime;

      Entities
        .WithAll<MoveForward>()
        .WithBurst()
        .ForEach((ref Translation pos, in Rotation rot, in MoveSpeed speed) =>
        {
          pos.Value = pos.Value + dt * speed.Value * math.forward(rot.Value);
        }).Schedule();
    }
  }
}
