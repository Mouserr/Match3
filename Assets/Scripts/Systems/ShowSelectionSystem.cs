using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Systems
{
	public class ShowSelectionSystem : ComponentSystem
	{
		private GameObject _selectionInstance;
		private EntityQuery _selectedGroup;

		public void Init(GameObject selection)
		{
			_selectionInstance = selection;
			_selectionInstance.gameObject.SetActive(false);
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			_selectedGroup = GetEntityQuery(ComponentType.ReadOnly<BallLink>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Selected>());
		}

		// TODO: call manual
		protected override void OnUpdate()
		{
			var selected = _selectedGroup.ToEntityArray(Allocator.TempJob);
			var translations = _selectedGroup.ToComponentDataArray<Translation>(Allocator.TempJob);

			if (selected.Length == 1)
			{
				_selectionInstance.gameObject.SetActive(true);
				_selectionInstance.transform.position = translations[0].Value;
			}
			else
			{
				_selectionInstance.gameObject.SetActive(false);
			}
		}
	}
}