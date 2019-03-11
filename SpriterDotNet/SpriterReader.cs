// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;
using SpriterDotNet.Parsers;
using SpriterDotNet.Preprocessors;
using System;

namespace SpriterDotNet
{
    /// <summary>
    /// Class responsible for getting a Spriter instance from a string input. It is also responsible for any processing logic such as initialization.
    /// This class basically contains no parsing / processing logic by itself but has collections of parsers / preprocessors to delegate the work to.
    ///
    /// For parsing, it iterates over all registered parses until a parser can parse the input string or until it reaches the end.
    ///
    /// For preprocessing, it invokes all the preprocessors in order.
    /// </summary>
    public class SpriterReader
    {
        /// <summary>
        /// An instance of the default Spriter reader.
        /// </summary>
        public static SpriterReader Default { get; private set; } = new SpriterReader();

        /// <summary>
        /// A static constructor for <see cref="SpriterReader"/>. Populates <see cref="Default"/> with a <see cref="XmlSpriterParser"/> and a <see cref="SpriterInitPreprocessor"/>.
        /// </summary>
        static SpriterReader()
        {
            Default.Parsers.Add(new XmlSpriterParser());
            Default.Preprocessors.Add(new SpriterInitPreprocessor());
        }

        /// <summary>
        /// Spriter parsers used by the reader.
        /// </summary>
        public ICollection<ISpriterParser> Parsers { get; set; } = new List<ISpriterParser>();

        /// <summary>
        /// Spriter preprocessors.
        /// </summary>
        public ICollection<ISpriterPreprocessor> Preprocessors { get; set; } = new List<ISpriterPreprocessor>();

        /// <summary>
        /// Parses and processes the provided data and returns a <see cref="Spriter"/>.
        /// </summary>
        /// <param name="data">The data to parse. Intaken as a <see cref="string"/>.</param>
        /// <returns>Returns a <see cref="Spriter"/></returns>
        public virtual Spriter Read(string data)
        {
            // Throw a null argument if the data is null.
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Trim the data of any leading and trailing white space.
            data = data.Trim();

            // Return null if the data is null or empty after being trimmed.
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            // Setup return Spriter.
            Spriter spriter = null;

            // Parse the data with any loaded parsers to find a Spriter.
            foreach (ISpriterParser parser in Parsers)
            {
                // If the data cannot be parsed by the current parser then continue on to the next parser without continuing past the if statement.
                if (!parser.CanParse(data))
                {
                    continue;
                }

                // Assign the spriter after parsing the parseable data.
                spriter = parser.Parse(data);

                // Exit the foreach loop once a Spriter is found.
                break;
            }

            // Process the Spriter with any loaded preprocessors.
            foreach (ISpriterPreprocessor preprocessor in Preprocessors)
            {
                preprocessor.Preprocess(spriter);
            }

            // Return a Spriter or null based on the usability of the provided data.
            return spriter;
        }
    }
}