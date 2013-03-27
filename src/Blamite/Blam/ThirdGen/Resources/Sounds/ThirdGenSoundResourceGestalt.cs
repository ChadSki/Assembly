﻿using System.Linq;
using Blamite.Blam.Resources.Sounds;
using Blamite.Blam.Util;
using Blamite.Flexibility;
using Blamite.IO;

namespace Blamite.Blam.ThirdGen.Resources.Sounds
{
	public class ThirdGenSoundResourceGestalt : ISoundResourceGestalt
	{
		public ThirdGenSoundResourceGestalt(StructureValueCollection values, IReader reader, FileSegmentGroup metaArea, BuildInformation buildInfo)
		{
			Load(values, reader, metaArea, buildInfo);
		}

		public StringID[] SoundNames { get; private set; }
		public ISoundPlayback[] SoundPlaybacks { get; private set; }
		public ISoundPermutation[] SoundPermutations { get; private set; }
		public ISoundRawChunk[] SoundRawChunks { get; private set; }

		private void Load(StructureValueCollection values, IReader reader, FileSegmentGroup metaArea, BuildInformation buildInfo)
		{
			LoadSoundNames(values, reader, metaArea, buildInfo);
			LoadSoundPlaybacks(values, reader, metaArea, buildInfo);
			LoadSoundPermutations(values, reader, metaArea, buildInfo);
			LoadSoundRawChunks(values, reader, metaArea, buildInfo);
		}
		private void LoadSoundNames(StructureValueCollection values, IReader reader, FileSegmentGroup metaArea, BuildInformation buildInfo)
		{
			var count = (int)values.GetInteger("number of sound names");
			var address = values.GetInteger("sound name table address");
			var layout = buildInfo.GetLayout("sound names");
			var entries = ReflexiveReader.ReadReflexive(reader, count, address, layout, metaArea);

			SoundNames = entries.Select(e => new StringID(e.GetInteger("name index"))).ToArray();
		}
		private void LoadSoundPlaybacks(StructureValueCollection values, IReader reader, FileSegmentGroup metaArea, BuildInformation buildInfo)
		{
			var count = (int)values.GetInteger("number of playbacks");
			var address = values.GetInteger("playback table address");
			var layout = buildInfo.GetLayout("sound playbacks");
			var entries = ReflexiveReader.ReadReflexive(reader, count, address, layout, metaArea);

			SoundPlaybacks = (from entry in entries
							  select new ThirdGenSoundPlayback(entry, SoundNames)).ToArray<ISoundPlayback>();
		}
		private void LoadSoundPermutations(StructureValueCollection values, IReader reader, FileSegmentGroup metaArea, BuildInformation buildInfo)
		{
			var count = (int)values.GetInteger("number of sound permutations");
			var address = values.GetInteger("sound permutation table address");
			var layout = buildInfo.GetLayout("sound permutations");
			var entries = ReflexiveReader.ReadReflexive(reader, count, address, layout, metaArea);

			SoundPermutations = (from entry in entries
								 select new ThirdGenSoundPermutation(entry, SoundNames)).ToArray<ISoundPermutation>();
		}
		private void LoadSoundRawChunks(StructureValueCollection values, IReader reader, FileSegmentGroup metaArea, BuildInformation buildInfo)
		{
			var count = (int)values.GetInteger("number of raw chunks");
			var address = values.GetInteger("raw chunk table address");
			var layout = buildInfo.GetLayout("sound raw chunks");
			var entries = ReflexiveReader.ReadReflexive(reader, count, address, layout, metaArea);

			SoundRawChunks = (from entry in entries
							  select new ThirdGenSoundRawChunk(entry)).ToArray<ISoundRawChunk>();
		}
	}
}
