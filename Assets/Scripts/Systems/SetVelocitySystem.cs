using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	public class SetVelocitySystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			Entities
				.WithNone<Velocity>()
				.ForEach((Entity entity, in Translation translation, in Destination destination, in Speed speed) =>
				{
					ecb.AddComponent(entity, new Velocity { Value = speed.Value * math.normalizesafe(destination.Value - translation.Value) });
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}