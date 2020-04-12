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
		private EntityQuery _gravityGroup;

		protected override void OnCreate()
		{
			base.OnCreate();
			_gravityGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.ReadOnly<Gravity>());
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var gravity = _gravityGroup.ToComponentDataArray<Gravity>(Allocator.TempJob);
			var gravityEntity = _gravityGroup.GetSingletonEntity();
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			Entities
				.ForEach((Entity entity, Destroyed destroyed, GravitySwitch gravitySwitch) =>
				{
					ecb.SetComponent(gravityEntity, new Gravity { Value = -gravity[0].Value });
				}).Run();
			
			ecb.Playback(EntityManager);
			ecb.Dispose();
			gravity.Dispose();
			return default;
		}
	}
}