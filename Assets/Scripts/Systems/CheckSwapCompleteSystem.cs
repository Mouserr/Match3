using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	public class CheckSwapCompleteSystem : ComponentSystem
	{
		private EntityQuery _swapQuery;

		protected override void OnCreate()
		{
			base.OnCreate();
			_swapQuery = GetEntityQuery(ComponentType.ReadOnly<SwapResult>(), ComponentType.ReadOnly<Swap>());
		}

		protected override void OnUpdate()
		{
			var result = _swapQuery.ToComponentDataArray<SwapResult>(Allocator.TempJob);

			if (!result[0].Success)
			{
				var swap = _swapQuery.ToComponentDataArray<Swap>(Allocator.TempJob);

				var fromBall = swap[0].From;
				var toBall = swap[0].To;

				var fromCell = EntityManager.GetComponentData<CellLink>(fromBall).Value;
				var toCell = EntityManager.GetComponentData<CellLink>(toBall).Value;

				EntityManager.SetComponentData(fromBall, new CellLink { Value = toCell });
				EntityManager.SetComponentData(toBall, new CellLink { Value = fromCell });

				EntityManager.AddComponentData(fromBall, new Destination { Value = EntityManager.GetComponentData<Translation>(toCell).Value });
				EntityManager.AddComponentData(toBall, new Destination { Value = EntityManager.GetComponentData<Translation>(fromCell).Value });

				EntityManager.SetComponentData(fromCell, new BallLink { Value = toBall });
				EntityManager.SetComponentData(toCell, new BallLink { Value = fromBall });

				swap.Dispose();
			}

			EntityManager.DestroyEntity(_swapQuery.GetSingletonEntity());

			result.Dispose();
		}
	}
}