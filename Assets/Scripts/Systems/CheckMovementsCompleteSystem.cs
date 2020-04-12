using Assets.Scripts.Components;
using Unity.Entities;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[AlwaysUpdateSystem]
	[UpdateAfter(typeof(MoveToDestinationSystem))]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class CheckMovementsCompleteSystem : ComponentSystem
	{
		private EntityQuery _movementsQuery;
		private EntityQuery _systemStateGroup;

		protected override void OnCreate()
		{
			base.OnCreate();
			_movementsQuery = GetEntityQuery(ComponentType.ReadOnly<Destination>());
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.Exclude<Delay>(), ComponentType.Exclude<NeedsToCheck>(), ComponentType.Exclude<Interactable>());
		}

		protected override void OnUpdate()
		{
			if (_systemStateGroup.IsEmptyIgnoreFilter || !_movementsQuery.IsEmptyIgnoreFilter)
			{
				return;
			}

			var systemState = _systemStateGroup.GetSingletonEntity();
			EntityManager.AddComponent<NeedsToCheck>(systemState);
		}
	}
}