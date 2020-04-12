using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
	[GenerateAuthoringComponent]
	public struct AutoRotated : IComponentData
	{
		public float Speed;
		public float3 Axis;
	}
}