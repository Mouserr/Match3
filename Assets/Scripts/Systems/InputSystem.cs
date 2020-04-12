using Assets.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	public class InputSystem : ComponentSystem
	{
		private Vector2Int? _start;
		private Plane _boardPlane = new Plane(-Vector3.forward, Vector3.zero);
		private Camera _camera;
		private Field _field;
		private EntityQuery _systemStateGroup;

		public void Init(Camera camera, Field field)
		{
			_camera = camera;
			_field = field;
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.ReadOnly<Interactable>(), ComponentType.Exclude<Delay>());
		}

		protected override void OnUpdate()
		{
			if (_systemStateGroup.IsEmptyIgnoreFilter)
			{
				return;
			}

			if (Input.GetMouseButtonUp(0))
			{
				var ray = _camera.ScreenPointToRay(Input.mousePosition);
				
				if (_boardPlane.Raycast(ray, out float distance))
				{
					var worldPosition = ray.GetPoint(distance);
					if (_field.TryGetCell(worldPosition, out var entity))
					{
						EntityManager.AddComponent<Selected>(entity);
					}
				}
			}
		}
	}
}