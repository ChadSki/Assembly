﻿using System;
using Blamite.Blam.Resources;
using Blamite.Blam.Resources.Models;
using Blamite.Blam.Resources.Sounds;
using Blamite.IO;

namespace Blamite.Blam.SecondGen
{
    // Just a dummy for now...
    public class SecondGenResourceMetaLoader : IResourceMetaLoader
    {
        public bool SupportsRenderModels
        {
            get { return false; }
        }
		public bool SupportsSounds
		{
			get { return false; }
		}

        public IRenderModel LoadRenderModelMeta(ITag modeTag, IReader reader)
        {
            throw new NotImplementedException();
        }
		public ISound LoadSoundMeta(ITag sndTag, IReader reader)
		{
			throw new NotImplementedException();
		}
	}
}
