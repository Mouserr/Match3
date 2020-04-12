using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Systems
{
	[AlwaysUpdateSystem]
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

		protected override void OnUpdate()
		{
			var selected = _selectedGroup.ToEntityArray(Allocator.TempJob);
			
			if (selected.Length == 1)
			{
				var translations = _selectedGroup.ToComponentDataArray<Translation>(Allocator.TempJob);
				_selectionInstance.gameObject.SetActive(true);
				var worldPos = translations[0].Value;
				_selectionInstance.transform.position = new Vector3(worldPos.x, worldPos.y, _selectionInstance.transform.position.z);
				translations.Dispose();
			}
			else
			{
				_selectionInstance.gameObject.SetActive(false);
			}

			selected.Dispose();
		}
	}
}