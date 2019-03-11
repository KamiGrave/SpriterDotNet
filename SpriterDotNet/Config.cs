// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet
{
    /// <summary>
    /// Spriter config file.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Enables ALL metadata calculations.
        /// </summary>
        public bool MetadataEnabled { get; set; } = true;

        /// <summary>
        /// Enables vars.
        /// </summary>
        public bool VarsEnabled { get; set; } = true;

        /// <summary>
        /// Enables tags.
        /// </summary>
        public bool TagsEnabled { get; set; } = true;

        /// <summary>
        /// Enables events.
        /// </summary>
        public bool EventsEnabled { get; set; } = true;

        /// <summary>
        /// Enables sounds.
        /// </summary>
        public bool SoundsEnabled { get; set; } = true;

        /// <summary>
        /// Enables object pooling.
        /// </summary>
        public bool PoolingEnabled { get; set; } = true;
    }
}