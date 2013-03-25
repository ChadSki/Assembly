﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blamite.Blam.Resources;
using Blamite.Flexibility;

namespace Blamite.Blam.ThirdGen.Resources
{
    public class ThirdGenResourcePage : IResourcePage
    {
        private ThirdGenCacheFileReference _externalFile;

        public ThirdGenResourcePage(StructureValueCollection values, ThirdGenCacheFileReference[] externalFiles)
        {
            Load(values, externalFiles);
        }

        /// <summary>
        /// Gets the path to the cache file that the resource is located in.
        /// Can be null if the resource is in the current file.
        /// </summary>
        public string FilePath
        {
            get { return (_externalFile != null) ? _externalFile.Path : null; }
        }

        /// <summary>
        /// Gets the offset of the resource page from the start of the cache file's resource pool.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the uncompressed size of the resource page.
        /// </summary>
        public int UncompressedSize { get; private set; }

        /// <summary>
        /// Gets the compressed size of the resource page.
        /// Can be the same as <see cref="UncompressedSize"/> if the page is not compressed.
        /// </summary>
        public int CompressedSize { get; private set; }

        /// <summary>
        /// Gets the method used to compress the resource page.
        /// </summary>
        public ResourcePageCompression CompressionMethod
        {
			get { return (CompressedSize != UncompressedSize) ? ResourcePageCompression.Deflate : ResourcePageCompression.None; }
        }

        private void Load(StructureValueCollection values, ThirdGenCacheFileReference[] externalFiles)
        {
            int fileIndex = (int)values.GetInteger("shared cache file index");
            if (fileIndex >= 0 && fileIndex < externalFiles.Length)
                _externalFile = externalFiles[fileIndex];

            Offset = (int)values.GetInteger("compressed block offset");
            CompressedSize = (int)values.GetInteger("compressed block size");
            UncompressedSize = (int)values.GetInteger("uncompressed block size");
        }
    }
}
