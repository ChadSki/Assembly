using System;
using Blamite.Blam.Resources.Models;
using Blamite.IO;
using Blamite.Blam.Resources.Sounds;

namespace Blamite.Blam.Resources
{
    /// <summary>
    /// Provides methods for loading resource metadata from tags.
    /// </summary>
    public interface IResourceMetaLoader
    {
        /// <summary>
        /// Gets whether or not renderable model metadata can be loaded with <see cref="LoadRenderModelMeta"/>.
        /// </summary>
        bool SupportsRenderModels { get; }

		/// <summary>
		/// Gets wether or not sound metadata can be loaded with <see cref="LoadSoundMeta"/>.
		/// </summary>
	    bool SupportsSounds { get; }

        /// <summary>
        /// Loads metadata for a renderable model from a mode tag.
        /// </summary>
        /// <param name="modeTag">The mode tag to load metadata from.</param>
        /// <param name="reader">The reader to read the data with.</param>
        /// <returns>An IRenderModel object holding the metadata in the tag. Can be null if loading failed.</returns>
        /// <exception cref="ArgumentException">Thrown if modeTag points to null data or is not from the mode class.</exception>
        /// <exception cref="NotSupportedException">Thrown if loading renderable model metadata is not supported.</exception>
        IRenderModel LoadRenderModelMeta(ITag modeTag, IReader reader);

		/// <summary>
		/// Loads metadata for a sound from a snd! tag.
		/// </summary>
		/// <param name="sndTag">The snd! tag to load metadata from.</param>
		/// <param name="reader">The reader to read the data with.</param>
		/// <returns>An ISound object holding the metadata of the tag. Can be null if loading failed.</returns>
		/// <exception cref="ArgumentException">Thrown if sndTag points to null data or is not from the snd! class.</exception>
		/// <exception cref="NotSupportedException">Thrown if loading sound metadata is not supported.</exception>
	    ISound LoadSoundMeta(ITag sndTag, IReader reader);
    }
}