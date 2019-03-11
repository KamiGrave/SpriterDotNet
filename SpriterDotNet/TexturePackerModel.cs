// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet
{
    /// <summary>
    /// A model for texture sheets used by Spriter. Contains <see cref="Meta"/>, <see cref="ImageInfo"/> and <see cref="Size"/> data.
    /// </summary>
    public class TexturePackerSheet
    {
        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s list of <see cref="ImageInfo"/>s.
        /// </summary>
        public List<ImageInfo> ImageInfos { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s meta data.
        /// </summary>
        public Meta Meta { get; set; }
    }

    /// <summary>
    /// The meta data for the <see cref="TexturePackerSheet"/>.
    /// </summary>
    public class Meta
    {
        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s app name.
        /// </summary>
        public string App { get; set; }

        /// <summary>
        /// <see cref="TexturePackerSheet"/>'s format.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s image path.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s current scale.
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s size.
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s version.
        /// </summary>
        public string Version { get; set; }
    }

    /// <summary>
    /// The image's meta data.
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s image name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines whether the <see cref="TexturePackerSheet"/>'s image is rotated.
        /// </summary>
        public bool Rotated { get; set; }

        /// <summary>
        /// Determines whether the <see cref="TexturePackerSheet"/>'s image has been trimmed.
        /// </summary>
        public bool Trimmed { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s frame <see cref="Size"/>.
        /// </summary>
        public Size Frame { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s source <see cref="Size"/>.
        /// </summary>
        public Size SourceSize { get; set; }

        /// <summary>
        /// The <see cref="TexturePackerSheet"/>'s <see cref="Spriter"/> source <see cref="Size"/>.
        /// </summary>
        public Size SpriteSourceSize { get; set; }
    }

    /// <summary>
    /// A class defining the X and Y axises along with the width and height of an object.
    /// </summary>
    public class Size
    {
        /// <summary>
        /// The position of the object along the X axis.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The position of the object along the Y axis.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The width of the object.
        /// </summary>
        public int W { get; set; }

        /// <summary>
        /// The height of the object.
        /// </summary>
        public int H { get; set; }
    }
}