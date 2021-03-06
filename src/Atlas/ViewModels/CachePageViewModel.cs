﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Atlas.Dialogs;
using Atlas.Helpers;
using Atlas.Helpers.Tags;
using Atlas.Models;
using Atlas.Views;
using Atlas.Views.Cache;
using Blamite.Blam;
using Blamite.Blam.Scripting;
using Blamite.Flexibility;
using Blamite.IO;
using Blamite.RTE;
using Blamite.RTE.H2Vista;
using Blamite.Util;
using Newtonsoft.Json;
using XBDMCommunicator;

namespace Atlas.ViewModels
{
	public class CachePageViewModel : Base
	{
		public CachePage CachePage { get; private set; }

		public static CachePageViewModel Singleton { get; private set; }

		public CachePageViewModel(CachePage cachePage)
		{
			Singleton = this;
			CachePage = cachePage;
			Editors = new ObservableCollection<ICacheEditor>();

			SetupXbdm();
		}

		#region Properties

		public EngineMemory EngineMemory
		{
			get { return _engineMemory; }
			set { SetField(ref _engineMemory, value); }
		}
		private EngineMemory _engineMemory;

		public EngineMemory.EngineVersion SelectedEngineMemoryVersion
		{
			get { return _selectedEngineMemoryVersion; }
			set { SetField(ref _selectedEngineMemoryVersion, value); }
		}
		private EngineMemory.EngineVersion _selectedEngineMemoryVersion;

		public string CacheLocation
		{
			get { return _cacheLocation; }
			private set { SetField(ref _cacheLocation, value); }
		}
		private string _cacheLocation;

		public ICacheFile CacheFile
		{
			get { return _cacheFile; }
			private set { SetField(ref _cacheFile, value); }
		}
		private ICacheFile _cacheFile;

		public EngineDescription EngineDescription
		{
			get { return _engineDescription; }
		}
		private EngineDescription _engineDescription;

		public IStreamManager MapStreamManager
		{
			get { return _mapStreamManager; }
			private set { SetField(ref _mapStreamManager, value); }
		}
		private IStreamManager _mapStreamManager;

		public Trie StringIdTrie
		{
			get { return _stringIdTrie; }
			private set { SetField(ref _stringIdTrie, value); }
		}
		private Trie _stringIdTrie;

		public TagHierarchy ClassHierarchy
		{
			get { return _classHierarchy; }
			private set { SetField(ref _classHierarchy, value); }
		}
		private TagHierarchy _classHierarchy;

		public IRteProvider RteProvider
		{
			get { return _rteProvider; }
			set { SetField(ref _rteProvider, value); }
		}
		private IRteProvider _rteProvider;

		public TagHierarchy ActiveHierarchy
		{
			get { return _activeHierarchy; }
			private set { SetField(ref _activeHierarchy, value); }
		}
		private TagHierarchy _activeHierarchy;

		public CacheHeaderInformation CacheHeaderInformation
		{
			get { return _cacheHeaderInformation; }
			private set { SetField(ref _cacheHeaderInformation, value); }
		}
		private CacheHeaderInformation _cacheHeaderInformation;

		#endregion

		#region Xbox Memory

		private Xbdm XboxDebugManager { get; set; }

		#endregion

		#region UI

		public bool BuildHasEngineMemory
		{
			get { return _buildHasEngineMemoryTools; }
			set { SetField(ref _buildHasEngineMemoryTools, value); }
		}
		private bool _buildHasEngineMemoryTools;

		public ObservableCollection<ICacheEditor> Editors
		{
			get { return _editors; }
			private set { SetField(ref _editors, value); }
		}
		private ObservableCollection<ICacheEditor> _editors;

		public ICacheEditor SelectedEditor
		{
			get { return _selectedEditor; }
			set { SetField(ref _selectedEditor, value); }
		}
		private ICacheEditor _selectedEditor;

		#endregion

		public void LoadCache(string cacheLocation)
		{
			//App.Storage.HomeWindowViewModel.AssemblyPage = null;
			var dialog = MetroBusyAlertBox.Show();

			var thread = new Thread(() =>
			{
				CacheLocation = cacheLocation;

				using (var fileStream = File.OpenRead(CacheLocation))
				{
					var fileInfo = new FileInfo(CacheLocation);
					var reader = new EndianReader(fileStream, Endian.BigEndian);
					CacheFile = CacheFileLoader.LoadCacheFile(reader, App.Storage.Settings.DefaultDatabase,
						out _engineDescription);

					MapStreamManager = new FileStreamManager(CacheLocation, reader.Endianness);

					StringIdTrie = new Trie();
					if (CacheFile.StringIDs != null)
						StringIdTrie.AddRange(CacheFile.StringIDs);

					// Set up RTE
					switch (CacheFile.Engine)
					{
						case EngineType.SecondGeneration:
							RteProvider = new H2VistaRteProvider("halo2.exe");
							break;

						case EngineType.ThirdGeneration:
							RteProvider = new XbdmRteProvider(XboxDebugManager);
							break;
					}

					LoadHeader();
					LoadTags();
					LoadEngineMemory();

					Application.Current.Dispatcher.Invoke(delegate
					{
						App.Storage.HomeWindowViewModel.UpdateStatus(
						   String.Format("{0} ({1})", CacheFile.InternalName, fileInfo.Name));

						dialog.ViewModel.CanClose = true;
						dialog.Close();
						App.Storage.HomeWindowViewModel.HideDialog();
						//App.Storage.HomeWindowViewModel.AssemblyPage = CachePage;
					});

				}
			});
			thread.Start();
		}

		private void LoadHeader()
		{
			CacheHeaderInformation = new CacheHeaderInformation
			{
				Game = EngineDescription.Name,
				Build = CacheFile.BuildString,
				Type = CacheFile.Type.ToString(),
				InternalName = CacheFile.InternalName,
				ScenarioName = CacheFile.ScenarioName,

				MetaBase = (CacheFile.MetaArea != null) ? "0x" + CacheFile.MetaArea.BasePointer.ToString("X8") : "Unknown",
				MetaSize = (CacheFile.MetaArea != null) ? "0x" + CacheFile.MetaArea.Size.ToString("X") : "Unknown",
				MapMagic = (CacheFile.MetaArea != null) ? "0x" + CacheFile.MetaArea.OffsetToPointer(0).ToString("X8") : "Unknown",
				IndexHeaderPointer = (CacheFile.RawTable != null) ? "0x" + CacheFile.IndexHeaderLocation.AsPointer().ToString("X8") : "Unknown",
				SdkVersion = (CacheFile.XDKVersion > 0) ? CacheFile.XDKVersion.ToString(CultureInfo.InvariantCulture) : "Unknown",
				RawTableOffset = (CacheFile.RawTable != null) ? "0x" + CacheFile.RawTable.Offset.ToString("X8") : "Unknown",
				RawTableSize = (CacheFile.RawTable != null) ? "0x" + CacheFile.RawTable.Size.ToString("X8") : "Unknown",
				IndexOffsetMagic = (CacheFile.LocaleArea != null) ? "0x" + ((uint)-CacheFile.LocaleArea.PointerMask).ToString("X8") : "Unknown"
			};
		}

		private void LoadTags()
		{
			if (CacheFile.TagClasses == null || CacheFile.Tags == null)
				return;

			ClassHierarchy = new ClassTagHierarchy(_cacheFile);
			PopulateHierarchy(_classHierarchy);
			Application.Current.Dispatcher.Invoke(delegate
			{
				switch (App.Storage.Settings.CacheEditorTagSortMethod)
				{
					case Settings.TagSort.TagClass:
						ActiveHierarchy = ClassHierarchy;
						break;

					case Settings.TagSort.PathHierarchy:
						ActiveHierarchy = new PathTagHierarchy(CacheFile);
						PopulateHierarchy(ActiveHierarchy);
						break;

					default:
						throw new NotImplementedException();
				}

				CachePage.TagTreeView.DataContext = ActiveHierarchy;
			});
		}

		private void LoadEngineMemory()
		{
			var memoryPath = String.Format(@"{0}/{1}/{2}/EngineMemory.json",
				VariousFunctions.GetApplicationLocation(), "Storage",
				EngineDescription.Settings.GetSettingOrDefault<string>("memory", null));

			try
			{
				if (!File.Exists(memoryPath))
					throw new FileNotFoundException();

				EngineMemory = JsonConvert.DeserializeObject<EngineMemory>(File.ReadAllText(memoryPath));
				if (EngineMemory.EngineVersions == null || EngineMemory.EngineVersions.Any())
					throw new InvalidDataException();

				SelectedEngineMemoryVersion = EngineMemory.EngineVersions.First();
				BuildHasEngineMemory = true;
			}
			catch
			{
				BuildHasEngineMemory = false;
			}
		}


		#region Modio Hex Codes (Editor Management)

		public void LoadTagEditor(TagHierarchyNode tagHierarchyNode)
		{
			var editor = new TagEditor(this, tagHierarchyNode);
			Editors.Add(editor);
			SelectedEditor = editor;
		}

		public void LoadScriptEditor(IScriptFile scriptFile)
		{
			ICacheEditor editor = null;
			foreach (var edi in from edi in Editors 
								let parsedEditor = edi as ScriptEditor 
								where parsedEditor != null 
								where parsedEditor.ViewModel.ScriptFile == scriptFile 
								select edi)
				editor = edi;

			if (editor == null)
			{
				editor = new ScriptEditor(this, scriptFile);
				Editors.Add(editor);
			}
			SelectedEditor = editor;
		}

		public void LoadAdvancedMemoryEditor()
		{
			var editor = Editors.FirstOrDefault(e => e is AdvancedMemoryEditor);
			if (editor == null)
			{
				editor = new AdvancedMemoryEditor(this);
				Editors.Add(editor);
			}
			SelectedEditor = editor;
		}

		public void LoadNetworkSessionEditor()
		{
			var editor = Editors.FirstOrDefault(e => e is NetworkSessionEditor);
			if (editor == null)
			{
				editor = new NetworkSessionEditor(this);
				Editors.Add(editor);
			}
			SelectedEditor = editor;
		}

		#endregion

		#region Helpers

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hierarchy"></param>
		private void PopulateHierarchy(TagHierarchy hierarchy)
		{
			foreach (var tag in _cacheFile.Tags.Where(t => t != null && t.Class != null && t.MetaLocation != null))
				hierarchy.AddTag(tag, GetTagName(tag));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public string GetTagName(ITag tag)
		{
			var name = _cacheFile.FileNames.GetTagName(tag);
			return string.IsNullOrWhiteSpace(name) ? tag.Index.ToString() : name;
		}

		#endregion

		#region XDK

		/// <summary>
		/// Sets up the Xbox 360 Developer Debug Manager
		/// </summary>
		private void SetupXbdm()
		{
			if (XboxDebugManager == null || XboxDebugManager.DeviceIdent != App.Storage.Settings.XdkIpAddress)
				XboxDebugManager = new Xbdm(App.Storage.Settings.XdkIpAddress);
		}

		public void FreezeConsole()
		{
			SetupXbdm();
			XboxDebugManager.Freeze();
		}

		public void UnfreezeConsole()
		{
			SetupXbdm();
			XboxDebugManager.Unfreeze();
		}

		public void RebootConsole(Xbdm.RebootType rebootType)
		{
			SetupXbdm();
			XboxDebugManager.Reboot(rebootType);
		}

		public byte PeekByte(UInt32 offset)
		{
			SetupXbdm();
			XboxDebugManager.MemoryStream.Flush();
			XboxDebugManager.MemoryStream.Seek(offset, SeekOrigin.Begin);
			return (byte)XboxDebugManager.MemoryStream.ReadByte();
		}

		public byte[] PeekBytes(UInt32 offset, int byteCount)
		{
			byte[] buffer = new byte[byteCount];

			SetupXbdm();
			XboxDebugManager.MemoryStream.Flush();
			XboxDebugManager.MemoryStream.Seek(offset, SeekOrigin.Begin);
			XboxDebugManager.MemoryStream.Read(buffer, 0x00, byteCount);

			return buffer;
		}

		// TODO: make this nicer m8
		public void PokeByte(UInt32 offset, byte value)
		{
			SetupXbdm();
			XboxDebugManager.MemoryStream.Seek(offset, SeekOrigin.Begin);
			XboxDebugManager.MemoryStream.WriteByte(value);
		}

		public void PokeBytes(UInt32 offset, byte[] data)
		{
			SetupXbdm();
			XboxDebugManager.MemoryStream.Seek(offset, SeekOrigin.Begin);
			XboxDebugManager.MemoryStream.Write(data, 0x00, data.Length);
		}

		#endregion
	}
}
