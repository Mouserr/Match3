using Unity.Entities;

namespace Assets.Scripts.Components
{
	[GenerateAuthoringComponent]
	public struct Speed : IComponentData
	{
		public float Value;
	}
}