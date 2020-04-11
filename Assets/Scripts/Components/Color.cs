using Unity.Entities;

namespace Assets.Scripts.Components
{
	[GenerateAuthoringComponent]
	public struct Color : IComponentData
	{
		public int Value;
	}
}