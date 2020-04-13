using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(LimitedSpawnSystem))]
	[UpdateBefore(typeof(FallSystem))]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class StopSpawnSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			ComponentDataFromEntity<Border> borders = GetComponentDataFromEntity<Border>(true);
			Entities
				.ForEach((Entity entity, in SpawnCount spawnCount, in SpawnLimit limit) =>
					{
						if (spawnCount.Value >= limit.Value)
						{
							ecb.RemoveComponent<SpawnLimit>(entity);
							ecb.RemoveComponent<Spawner>(entity);
							if (borders.Exists(entity))
							{
								var border = borders[entity];
								ecb.AddComponent(entity, new Spawner { Offset = new float3(0, border.RequiredGravity.y > 0 ? -1 : 1, 0)});
							}
						}
					}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}