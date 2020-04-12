using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
	public struct Gravity : IComponentData
	{
		public int2 Value;
	}
}