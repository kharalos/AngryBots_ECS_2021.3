using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(MoveForwardSystem))]
[UpdateBefore(typeof(TimedDestroySystem))]
public partial class CollisionSystem : SystemBase
{
  private EntityQuery enemyGroup;
  private EntityQuery bulletGroup;
  private EntityQuery playerGroup;

  protected override void OnCreate()
  {
    playerGroup = GetEntityQuery(typeof(Health), ComponentType.ReadOnly<Translation>(),
      ComponentType.ReadOnly<PlayerTag>());
    enemyGroup = GetEntityQuery(typeof(Health), ComponentType.ReadOnly<Translation>(),
      ComponentType.ReadOnly<EnemyTag>());
    bulletGroup = GetEntityQuery(typeof(TimeToLive), ComponentType.ReadOnly<Translation>());
  }

  [BurstCompile]
  private struct CollisionJob : IJobChunk
  {
    public float radius;

    public ComponentTypeHandle<Health> healthType;
    [ReadOnly] public ComponentTypeHandle<Translation> translationType;

    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> transToTestAgainst;


    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    {
      var chunkHealths = chunk.GetNativeArray(healthType);
      var chunkTranslations = chunk.GetNativeArray(translationType);

      for (var i = 0; i < chunk.Count; i++)
      {
        var damage = 0f;
        var health = chunkHealths[i];
        var pos = chunkTranslations[i];

        for (var j = 0; j < transToTestAgainst.Length; j++)
        {
          var pos2 = transToTestAgainst[j];

          if (CheckCollision(pos.Value, pos2.Value, radius)) damage += 1;
        }

        if (damage > 0)
        {
          health.Value -= damage;
          chunkHealths[i] = health;
        }
      }
    }
  }

  protected override void OnUpdate()
  {
    var healthType = GetComponentTypeHandle<Health>();
    var translationType = GetComponentTypeHandle<Translation>(true);

    var enemyRadius = Settings.EnemyCollisionRadius;
    var playerRadius = Settings.PlayerCollisionRadius;

    var jobEvB = new CollisionJob
    {
      radius = enemyRadius * enemyRadius,
      healthType = healthType,
      translationType = translationType,
      transToTestAgainst = bulletGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
    };

    var jobHandle = jobEvB.Schedule(enemyGroup);

    if (Settings.IsPlayerDead())
    {
      jobHandle.Complete();
      return;
    }

    var jobPvE = new CollisionJob
    {
      radius = playerRadius * playerRadius,
      healthType = healthType,
      translationType = translationType,
      transToTestAgainst = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
    };

    var PveJobHandle2 = jobPvE.Schedule(playerGroup, jobHandle);
    jobHandle.Complete();
    PveJobHandle2.Complete();
  }

  private static bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
  {
    var delta = posA - posB;
    var distanceSquare = delta.x * delta.x + delta.z * delta.z;

    return distanceSquare <= radiusSqr;
  }
}
