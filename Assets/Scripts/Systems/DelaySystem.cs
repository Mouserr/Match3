using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	public class DelaySystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var deltaTime = Time.DeltaTime;
			Entities
				.ForEach((Entity entity, ref Delay delay) =>
				{
					delay.Value -= deltaTime;
					if (delay.Value <= 0)
					{
						ecb.RemoveComponent<Delay>(entity);
					}
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}