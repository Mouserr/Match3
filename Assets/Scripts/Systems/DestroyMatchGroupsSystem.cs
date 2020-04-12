using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(FindMatchesSystem))]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class DestroyMatchGroupsSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			Entities
				.ForEach((Entity entity, DynamicBuffer<MatchGroupElement> matchGroup) =>
				{
					for (int i = 0; i < matchGroup.Length; i++)
					{
						ecb.AddComponent<Destroyed>(matchGroup[i].BallEntity);
					}

					ecb.AddComponent<Destroyed>(entity);
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}