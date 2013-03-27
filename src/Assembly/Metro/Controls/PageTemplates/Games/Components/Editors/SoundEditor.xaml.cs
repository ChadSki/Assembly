using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows.Controls;
using Assembly.Helpers;
using Blamite.Blam;
using Blamite.Blam.Resources;
using Blamite.Flexibility;
using Blamite.IO;
using Blamite.Blam.Resources.Sounds;
using Assembly.Metro.Dialogs;
using System.ComponentModel;
using System.IO;

namespace Assembly.Metro.Controls.PageTemplates.Games.Components.Editors
{
	/// <summary>
	/// Interaction logic for SoundEditor.xaml
	/// </summary>
	public partial class SoundEditor : INotifyPropertyChanged
	{
		private readonly TagEntry _tag;
        private readonly ICacheFile _cache;
	    private readonly IStreamManager _streamManager;

		private ObservableCollection<ViewPermutation> _permutations = new ObservableCollection<ViewPermutation>();
		public ObservableCollection<ViewPermutation> Permutations
		{
			get { return _permutations; }
			set 
			{ 
				_permutations = value;
				NotifyPropertyChanged("Permutations");
			}
		}
		public class ViewPermutation
		{
			public string Name { get; set; }
			public int Index { get; set; }
			public ISoundPermutation SoundPermutation { get; set; }
		}

		private readonly ISound _sound;
		private readonly IResourceTable _resourceTable;
		private readonly IResource _resource;
		private readonly IResourcePage[] _resourcePages;
		private readonly ISoundResourceGestalt _soundResourceGestalt;
		private readonly Dictionary<int, ISoundPermutation> _soundPermutations;

		private MediaElement _mediaElement;

		public SoundEditor(TagEntry tag, ICacheFile cache, IStreamManager streamManager)
		{
			InitializeComponent();

            _tag = tag;
            _cache = cache;
	        _streamManager = streamManager;
			_mediaElement = new MediaElement();

			// Set Tagname
			lblTagName.Text = tag.TagFileName;

			// Sound
			if (_cache.ResourceMetaLoader.SupportsSounds)
			{
				using (var stream = _streamManager.OpenRead())
				{
					using (var reader = new EndianReader(stream, Endian.BigEndian))
					{
						_soundResourceGestalt = _cache.LoadSoundResourceGestalt(reader);
						_sound = _cache.ResourceMetaLoader.LoadSoundMeta(_tag.RawTag, reader);
						_resourceTable = _cache.LoadResourceTable(reader);
						_resource = _resourceTable[_sound.ResourceIndex];
						_resourcePages = new[]
						                 {
											 _resource.PrimaryPage,
											 _resource.SecondaryPage
						                 };
					}
				}

				// Permutations
				var playback = _soundResourceGestalt.SoundPlaybacks[_sound.PlaybackIndex];
				_soundPermutations = new Dictionary<int, ISoundPermutation>();
				for (var i = playback.FirstPermutationIndex; i < playback.FirstPermutationIndex + playback.EncodedPermutationCount; i++)
				{
					_soundPermutations.Add(i, _soundResourceGestalt.SoundPermutations[i]);
					Permutations.Add(new ViewPermutation
						                 {
											Name = _cache.StringIDs.GetString(_soundResourceGestalt.SoundPermutations[i].SoundName),
											Index = i,
											SoundPermutation = _soundResourceGestalt.SoundPermutations[i]
						                 });
				}
				lbSoundPermutations.DataContext = Permutations;

				// Load Resource Page 1
				if (_resourcePages[0] != null)
				{
					var page = _resourcePages[0];

					lblResourcePage1Compression.Text = page.CompressionMethod.ToString();
					lblResourcePage1CompressedSize.Text = "0x" + page.CompressedSize.ToString("X8");
					lblResourcePage1UncompressedSize.Text = "0x" + page.UncompressedSize.ToString("X8");
					lblResourcePage1Offset.Text = "0x" + page.Offset.ToString("X8");
					lblResourcePage1FilePath.Text = page.FilePath ?? "maps\\" + _cache.InternalName + ".map";
				}

				// Load Resource Page 2
				if (_resourcePages[1] != null)
				{
					var page = _resourcePages[1];

					lblResourcePage2Compression.Text = page.CompressionMethod.ToString();
					lblResourcePage2CompressedSize.Text = "0x" + page.CompressedSize.ToString("X8");
					lblResourcePage2UncompressedSize.Text = "0x" + page.UncompressedSize.ToString("X8");
					lblResourcePage2Offset.Text = "0x" + page.Offset.ToString("X8");
					lblResourcePage2FilePath.Text = page.FilePath ?? "maps\\" + _cache.InternalName + ".map";
				}

				// Load Sound Info
				if (_sound != null)
				{
					lblSoundInfoSoundClass.Text = _sound.SoundClass.ToString(CultureInfo.InvariantCulture);
					lblSoundInfoAudioChannel.Text = _sound.AudioChannel.ToString();
					lblSoundInfoEncoding.Text = _sound.Encoding.ToString();
					lblSoundInfoMaxPlaytime.Text = _sound.MaxPlaytime.ToString(CultureInfo.InvariantCulture);
				}
			}
			else
			{
				IsEnabled = false;
				MetroMessageBox.Show("Unsupported", "Assembly doesn't support sounds on this build of Halo yet.");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(string info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		private void btnPlay_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var perm = (ViewPermutation)lbSoundPermutations.SelectedValue;
			var sound = ExtractPerm(perm, false);
			var xmaConverter = Blamite.Properties.Resources.towav;
			var tempFile = Path.GetTempFileName();
			File.WriteAllBytes(tempFile, sound);
			File.WriteAllBytes(VariousFunctions.GetApplicationLocation() + "towav.exe", xmaConverter);

			VariousFunctions.RunProgramSilently(VariousFunctions.GetApplicationLocation() + "towav.exe",
			                                    string.Format("\"{0}\"", Path.GetFileName(tempFile)),
			                                    Path.GetDirectoryName(tempFile));

			var simpleSound = new SoundPlayer(Path.ChangeExtension(tempFile, "wav"));
			simpleSound.Play();
		}
		private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (_mediaElement != null)
				_mediaElement.Stop();
		}
		private void btnExtractSelectedPerm_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var perm = (ViewPermutation) lbSoundPermutations.SelectedValue;
			var sound = ExtractPerm(perm, false);

			if (File.Exists(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\" + perm.Name + ".xma"))
				File.Delete(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\" + perm.Name + ".xma");

			File.WriteAllBytes(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\" + perm.Name + ".xma", sound);
		}
		private void btnExtractAllPerms_Click(object sender, System.Windows.RoutedEventArgs e)
		{

		}
		private void btnExtractRawSound_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var name = _tag.TagFileName.Remove(0, _tag.TagFileName.LastIndexOf('\\') + 1);
			var sound = ExtractPerm();

			if (File.Exists(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\" + name + ".xma"))
				File.Delete(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\" + name + ".xma");

			File.WriteAllBytes(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\" + name + ".xma", sound);
		}

		private byte[] ExtractPerm(ViewPermutation permutation = null, bool extractRawOnly = true)
		{
			var outputBytes = new List<byte>();

			using (var cacheReader = new EndianReader(_streamManager.OpenRead(), Endian.BigEndian))
			{
				Stream resourceStream = null;
				foreach (var page in _resourcePages)
				{
					if (page == null) continue;

					ICacheFile resourceCache = null;
					EndianReader resourceReader = null;

					if (page.FilePath != null)
					{
						// Load Resource Map
						var resourceMapName = page.	FilePath.Remove(0, page.FilePath.LastIndexOf('\\') + 1).Replace(".map", "");
						if (!File.Exists(@"A:\Xbox\Games\Halo 3\Maps\Clean\" + resourceMapName + ".map"))
							throw new IOException("Required Resource Cache could not be found. (" + page.FilePath + ").");

						// Load Build Info
						var formatsPath = Path.Combine(VariousFunctions.GetApplicationLocation(), "Formats");
						var supportedBuildsPath = Path.Combine(formatsPath, "SupportedBuilds.xml");
						var _layoutLoader = new BuildInfoLoader(supportedBuildsPath, formatsPath);

						if (resourceStream == null)
						{
							resourceStream = new FileStream(@"A:\Xbox\Games\Halo 3\Maps\Clean\" + resourceMapName + ".map", FileMode.Open);
							resourceReader = new EndianReader(resourceStream, Endian.BigEndian);
							resourceCache = CacheFileLoader.LoadCacheFile(resourceReader, _layoutLoader);
						}
					}

					var tmpStream = new MemoryStream();
					var extractor = new ResourcePageExtractor(resourceCache ?? _cache);
					extractor.ExtractPage(page, resourceCache == null ? cacheReader.BaseStream : resourceReader.BaseStream, tmpStream);

					tmpStream.Position = 0x00;
					var bytes = VariousFunctions.StreamToByteArray(tmpStream);
					if (bytes.Length > 0)
						outputBytes.AddRange(new List<byte>(bytes));
				}
				if (resourceStream != null) 
					resourceStream.Close();
			}

			if (outputBytes.Count == 0)
				return null;

			// Get Perm from sound
			if (permutation != null)
			{
				var rawChunk = _soundResourceGestalt.SoundRawChunks[permutation.SoundPermutation.RawChunkIndex];

				outputBytes.RemoveRange(0, rawChunk.Offset);
				outputBytes.RemoveRange(rawChunk.Size, outputBytes.Count - rawChunk.Size);
			}

			if (extractRawOnly)
			{
				return outputBytes.ToArray<byte>();
			}
			
			
			if (_sound.Encoding == Encoding.XMA)
			{
				using (var convertStream = new EndianStream(new MemoryStream(), Endian.BigEndian))
				{
					// Generate an XMA header
					// ADAPTED FROM wwisexmabank - I DO NOT TAKE ANY CREDIT WHATSOEVER FOR THE FOLLOWING CODE.
					// See http://hcs64.com/vgm_ripping.html for more information

					// 'riff' chunk
					convertStream.WriteInt32(0x52494646); // 'RIFF'
					convertStream.Endianness = Endian.LittleEndian;
					convertStream.WriteInt32(0x00);
					convertStream.Endianness = Endian.BigEndian;
					convertStream.WriteInt32(0x57415645);

					// 'data' chunk
					convertStream.WriteInt32(0x64617461); // 'data'
					convertStream.Endianness = Endian.LittleEndian;
					convertStream.WriteInt32(outputBytes.Count);
					convertStream.WriteBlock(outputBytes.ToArray());

					// 'xma2' footer
					switch (_sound.AudioChannel)
					{
						case AudioChannel.Mono:
							{
								var mono_footer = new byte[]
									                  {
										                  0x58, 0x4d, 0x41, 0x32, 0x2c, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0xff, 0x00, 0x00, 0x01, 0x80,
										                  0x00, 0x00, 0x8a, 0x00, 0x00, 0x00, 0xab, 0xD2, 0x00, 0x00, 0x10, 0xd6, 0x00, 0x00, 0x3d, 0x14,
										                  0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x8a, 0x00, 0x00, 0x00, 0x88, 0x80, 0x00, 0x00, 0x00, 0x01,
										                  0x01, 0x00, 0x00, 0x01, 0x73, 0x65, 0x65, 0x6b, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x8a, 0x00
									                  };
								convertStream.WriteBlock(mono_footer);
							}
							break;
						case AudioChannel.Stereo:
							{
								var mono_footer = new byte[]
									                  {
										                  0x58, 0x4d, 0x41, 0x32, 0x2c, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0xff, 0x00, 0x00, 0x01, 0x80, 
														  0x00, 0x01, 0x0f, 0x80, 0x00, 0x00, 0xac, 0x44, 0x00, 0x00, 0x10, 0xd6, 0x00, 0x00, 0x3d, 0x14, 
														  0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x00, 0x01, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x01, 
														  0x02, 0x00, 0x02, 0x01, 0x73, 0x65, 0x65, 0x6b, 0x04, 0x00, 0x00, 0x00, 0x00, 0x01, 0x10, 0x00
													  };
								convertStream.WriteBlock(mono_footer);
							}
							break;
					}

					// write length to 'riff' chunk
					convertStream.Endianness = Endian.LittleEndian;
					convertStream.SeekTo(0x04);
					convertStream.WriteInt32((int)convertStream.Length - 0x08);

					convertStream.SeekTo(0x00);
					return VariousFunctions.StreamToByteArray(convertStream.BaseStream);
				}
			}

			// no encoding data, throw
			throw new NotSupportedException("This encoding stype is not supported.");
		}
	}
}


/*
using (var reader = new EndianReader(_streamManager.OpenRead(), Endian.BigEndian))
{
	using (var outStream = new FileStream(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\sound.xma", FileMode.OpenOrCreate))
	{
		var sound = _cache.ResourceMetaLoader.LoadSoundMeta(_tag.RawTag, reader);
		var table = _cache.LoadResourceTable(reader);
		var resource = table[sound.ResourceIndex];
		var page = (resource.PrimaryPage.UncompressedSize > 0) ? resource.PrimaryPage : resource.SecondaryPage;
		ICacheFile resourceCache = null;
		EndianReader resourceReader = null;
		FileStream resourceStream = null;
		if (page.FilePath != null)
		{
			// Load Resource Map
			var resourceMapName = page.FilePath.Remove(0, page.FilePath.LastIndexOf('\\') + 1).Replace(".map", "");
			if (!File.Exists(@"A:\Xbox\Games\Halo 3\Maps\Clean\" + resourceMapName + ".map"))
				throw new IOException("Resource Cache could not be found.");

			// Load Build Info
			var formatsPath = Path.Combine(VariousFunctions.GetApplicationLocation(), "Formats");
			var supportedBuildsPath = Path.Combine(formatsPath, "SupportedBuilds.xml");
			var _layoutLoader = new BuildInfoLoader(supportedBuildsPath, formatsPath);

			resourceStream = new FileStream(@"A:\Xbox\Games\Halo 3\Maps\Clean\" + resourceMapName + ".map", FileMode.Open);
			resourceReader = new EndianReader(resourceStream, Endian.BigEndian);
			resourceCache = CacheFileLoader.LoadCacheFile(resourceReader, _layoutLoader);
		}

		var extractor = new ResourcePageExtractor(resourceCache ?? _cache);
		extractor.ExtractPage(page, resourceCache == null ? reader.BaseStream : resourceReader.BaseStream, outStream);

		if (page.CompressionMethod == ResourcePageCompression.None)
		{
							
		}
						

		if (resourceStream != null)
			resourceStream.Close();
	}
}
*/