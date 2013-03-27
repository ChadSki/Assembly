namespace Blamite.Blam.Resources.Sounds
{
	/// <summary>
	/// Types of SampleRate that a Sound can have.
	/// </summary>
	public enum AudioChannel
	{
		Mono,
		Stereo
	}

	/// <summary>
	/// Types of Encoding that a Sound can have.
	/// </summary>
	public enum Encoding
	{
		XMA,
		XboxADPCM,
		WMA = 0x04
	}

	/// <summary>
	/// A sound that can be played in the game.
	/// </summary>
	public interface ISound
	{
		/// <summary>
		/// Gets the Sound Class of the Sound.
		/// </summary>
		int SoundClass { get; }
		
		/// <summary>
		/// Gets the Audio Channel of the Sound.
		/// </summary>
		AudioChannel AudioChannel { get; }

		/// <summary>
		/// Gets the Encoding of the Sound.
		/// </summary>
		Encoding Encoding { get; }

		/// <summary>
		/// Gets the Index of the Codec of the Sound.
		/// </summary>
		int CodecIndex { get; }

		/// <summary>
		/// Gets the Index of the Playback of the Sound.
		/// </summary>
		int PlaybackIndex { get; }

		/// <summary>
		/// Gets the Permutation Chunk Count of the Sound.
		/// </summary>
		int PermutationChunkCount { get; }

		/// <summary>
		/// Gets the datum index of the Sound.
		/// </summary>
		/// <seealso cref="IResourceTable"/>
		DatumIndex ResourceIndex { get; }

		/// <summary>
		/// Gets the Max Playtime of the Sound.
		/// </summary>
		int MaxPlaytime { get; }
	}
}