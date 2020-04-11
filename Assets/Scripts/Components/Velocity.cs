using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
	public struct Velocity : IComponentData
	{
		public float3 Value;
	}
}