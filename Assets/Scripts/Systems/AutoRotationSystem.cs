using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	public class AutoRotationSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var deltaTime = Time.DeltaTime;
			Entities
				.ForEach((Entity entity, ref Rotation rotation, in AutoRotated autoRotated) =>
				{
					rotation.Value = math.mul(
						math.normalize(rotation.Value),
						quaternion.AxisAngle(autoRotated.Axis, autoRotated.Speed * deltaTime));
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}