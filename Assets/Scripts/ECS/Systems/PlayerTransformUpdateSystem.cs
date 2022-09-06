using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[UpdateBefore(typeof(CollisionSystem))]
public partial class PlayerTransformUpdateSystem : SystemBase
{
  protected override void OnUpdate()
  {
    Entities
      .WithoutBurst()
      .WithAll<PlayerTag>()
      .ForEach((ref Translation pos) =>
      {
        if (Settings.IsPlayerDead())
          return;
        pos.Value = Settings.PlayerPosition;
      }).Run();
  }
}
