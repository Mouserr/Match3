using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(MatchLogicGroup))]
	public class DestroySystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			Entities
				.ForEach((Entity entity, Destroyed destroyed) =>
				{
					ecb.DestroyEntity(entity);
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}