using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(DestroyBallsSystem))]
	[UpdateBefore(typeof(DestroySystem))]
	public class GravitySwitchSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			ComponentDataFromEntity<Gravity> gravities = GetComponentDataFromEntity<Gravity>(true);
			var gravityEntity = GetSingletonEntity<SystemState>();
			var gravity = gravities[gravityEntity];
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			Entities
				.ForEach((Entity entity, Destroyed destroyed, GravitySwitch gravitySwitch) =>
				{
					ecb.SetComponent(gravityEntity, new Gravity { Value = -gravity.Value });
				}).Run();
			
			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}