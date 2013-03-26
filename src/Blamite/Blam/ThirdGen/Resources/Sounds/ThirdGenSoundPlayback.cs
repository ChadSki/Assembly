using Blamite.Blam.Resources.Sounds;
using Blamite.Flexibility;

namespace Blamite.Blam.ThirdGen.Resources.Sounds
{
	public class ThirdGenSoundPlayback : ISoundPlayback
	{
		public ThirdGenSoundPlayback(StructureValueCollection values, StringID[] stringIds)
		{
			Load(values, stringIds);
		}

		public StringID SoundName { get; private set; }

		public int ParametersIndex { get; private set; }

		public int FirstRuntimePermutationFlagIndex { get; private set; }

		public int EncodedPermutationData { get; private set; }

		public int FirstPermutationIndex { get; private set; }

		private void Load(StructureValueCollection values, StringID[] stringIds)
		{
			SoundName = stringIds[values.GetInteger("sound name index")];
			ParametersIndex = (int)values.GetInteger("parameters index");
			FirstRuntimePermutationFlagIndex = (int)values.GetInteger("first runtime permutation flag index");
			EncodedPermutationData = (int)values.GetInteger("encoded permutation data");
			FirstPermutationIndex = (int)values.GetInteger("first permutation index");
		}
	}
}
