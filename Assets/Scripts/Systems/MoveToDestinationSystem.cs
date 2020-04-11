using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(SetVelocitySystem))]
	public class MoveToDestinationSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var deltaTime = Time.DeltaTime;
			Entities
				.ForEach((Entity entity, ref Translation translation, in Destination destination, in Velocity velocity) =>
				{
					var newTranslation = translation.Value + velocity.Value * deltaTime;

					if (math.lengthsq(destination.Value - newTranslation) < 0.001f || math.dot(newTranslation - translation.Value, destination.Value - translation.Value) <= 0)
					{
						newTranslation = destination.Value;
						ecb.RemoveComponent<Velocity>(entity);
						ecb.RemoveComponent<Destination>(entity);
					}

					translation.Value = newTranslation;
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}