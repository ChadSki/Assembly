using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Assembly.Helpers;
using Blamite.Blam;
using Blamite.Blam.Resources;
using Blamite.Blam.SecondGen;
using Blamite.Flexibility;
using Blamite.IO;
using Blamite.RTE;
using Blamite.Util;
using System;

namespace Assembly.Metro.Controls.PageTemplates.Games.Components
{
    /// <summary>
    /// Interaction logic for MetaContainer.xaml
    /// </summary>
    public partial class MetaContainer
    {
        private TagEntry _tag;
        private BuildInformation _buildInfo;
        private ICacheFile _cache;
	    private IStreamManager _streamManager;
	    private IRTEProvider _rteProvider;
        private Trie _stringIDTrie;
	    private TagHierarchy _tags;

		private MetaInformation _metaInformation;
		private MetaEditor _metaEditor;
		private PluginEditor _pluginEditor;

        #region Public Access
        public TagEntry TagEntry
        {
            get { return _tag; }
            set { _tag = value; }
        }
        #endregion

        public MetaContainer(BuildInformation buildInfo, TagEntry tag, TagHierarchy tags, ICacheFile cache, IStreamManager streamManager, IRTEProvider rteProvider, Trie stringIDTrie)
        {
            InitializeComponent();

            _tag = tag;
	        _tags = tags;
            _buildInfo = buildInfo;
            _cache = cache;
	        _streamManager = streamManager;
	        _rteProvider = rteProvider;
            _stringIDTrie = stringIDTrie;

            tbMetaEditors.SelectedIndex = (int)Settings.halomapLastSelectedMetaEditor;

            // Create Meta Information Tab
            _metaInformation = new MetaInformation(_buildInfo, _tag, _cache);
			tabTagInfo.Content = _metaInformation;

            // Create Meta Editor Tab
			_metaEditor = new MetaEditor(_buildInfo, _tag, this, _tags, _cache, _streamManager, _rteProvider, _stringIDTrie)
				              {
					              Padding = new Thickness(0)
				              };
	        tabMetaEditor.Content = _metaEditor;

            // Create Plugin Editor Tab
            _pluginEditor = new PluginEditor(_buildInfo, _tag, this, _metaEditor);
            tabPluginEditor.Content = _pluginEditor;

			// Sounds
			if (_cache.ResourceMetaLoader.SupportsSounds)
			{
				tabSound.Visibility = Visibility.Visible;

				using (var reader = new EndianReader(_streamManager.OpenRead(), Endian.BigEndian))
				{
					using (var outStream = new FileStream(@"A:\Xbox\Games\Halo 3\Maps\Modded\waste\sound.xma", FileMode.OpenOrCreate))
					{
						var sound = _cache.ResourceMetaLoader.LoadSoundMeta(_tag.RawTag, reader);
						var table = _cache.LoadResourceTable(reader);
						var resource = table[sound.ResourceIndex];
						var page = resource.PrimaryPage;
						var resourceMapName = page.FilePath.Remove(0, page.FilePath.LastIndexOf('\\') + 1).Replace(".map", "");
						ICacheFile resourceCache = null;
						EndianReader resourceReader = null;
						FileStream resourceStream = null;
						if (resourceMapName != _cache.InternalName)
						{
							// Load Resource Map
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

						var endOffset = int.MaxValue;
						foreach (var res in table.Where(res => res.PrimaryOffset > resource.PrimaryOffset && res.PrimaryOffset < endOffset))
							endOffset = res.PrimaryOffset;

						var length = endOffset - resource.PrimaryOffset;

						if (resourceStream != null)
							resourceStream.Close();
					}
				}
			}
			else
				if (Settings.halomapLastSelectedMetaEditor == Settings.LastMetaEditorType.Audio)
					tbMetaEditors.SelectedIndex = (int) Settings.LastMetaEditorType.MetaEditor;
        }

		public void GoToRawPluginLine(int pluginLine)
		{
			tbMetaEditors.SelectedIndex = (int)Settings.LastMetaEditorType.PluginEditor;
			_pluginEditor.GoToLine(pluginLine);

		}

		public void LoadNewTagEntry(TagEntry tag)
		{
			TagEntry = tag;

			// Create Meta Information Tab
			_metaInformation = new MetaInformation(_buildInfo, _tag, _cache);
			tabTagInfo.Content = _metaInformation;

			// Create Meta Editor Tab
			_metaEditor.LoadNewTagEntry(tag);

			// Create Plugin Editor Tab
			_pluginEditor = new PluginEditor(_buildInfo, _tag, this, _metaEditor);
			tabPluginEditor.Content = _pluginEditor;
		}

        private void tbMetaEditors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.halomapLastSelectedMetaEditor = (Settings.LastMetaEditorType)tbMetaEditors.SelectedIndex;
        }
    }
}
