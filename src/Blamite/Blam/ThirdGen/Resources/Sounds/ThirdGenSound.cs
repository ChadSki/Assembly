using Blamite.Blam.Resources.Sounds;
using Blamite.Flexibility;

namespace Blamite.Blam.ThirdGen.Resources.Sounds
{
	public class ThirdGenSound : ISound
	{
		public ThirdGenSound(StructureValueCollection values)
		{
			Load(values);
		}

		public int SoundClass { get; private set; }

		public AudioChannel AudioChannel { get; private set; }

		public Encoding Encoding { get; private set; }

		public int CodecIndex { get; private set; }

		public int PlaybackIndex { get; private set; }

		public int PermutationChunkCount { get; private set; }

		public DatumIndex ResourceIndex { get; private set; }

		public int MaxPlaytime { get; private set; }

		private void Load(StructureValueCollection values)
		{
			SoundClass = (byte) values.GetInteger("sound class");
			AudioChannel = (AudioChannel)values.GetInteger("audio channel");
			Encoding = (Encoding)values.GetInteger("encoding");
			CodecIndex = (byte)values.GetInteger("codec index");
			PlaybackIndex = (short)values.GetInteger("playback index");
			PermutationChunkCount = (byte)values.GetInteger("permutation chunk count");
			ResourceIndex = new DatumIndex(values.GetInteger("resource datum index"));
			MaxPlaytime = (short)values.GetInteger("max playtime");
		}
	}
}
