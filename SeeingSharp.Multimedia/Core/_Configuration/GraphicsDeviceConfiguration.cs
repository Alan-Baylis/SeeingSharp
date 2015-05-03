﻿#region License information (SeeingSharp and all based games/applications)
/*
    SeeingSharp and all games/applications based on it (more info at http://www.rolandk.de/wp)
    Copyright (C) 2015 Roland König (RolandK)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.ComponentModel;

namespace SeeingSharp.Multimedia.Core
{
    public class GraphicsDeviceConfiguration
    {
        private GraphicsCoreConfiguration m_coreConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDeviceConfiguration" /> class.
        /// </summary>
        /// <param name="coreConfig">The core configuration object.</param>
        public GraphicsDeviceConfiguration(GraphicsCoreConfiguration coreConfig)
        {
            m_coreConfig = coreConfig;
        }

        /// <summary>
        /// Gets or sets the texture quality level.
        /// </summary>
        [Browsable(true)]
        public TextureQuality TextureQuality
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the geometry quality level.
        /// </summary>
        [Browsable(true)]
        public GeometryQuality GeometryQuality
        {
            get;
            set;
        }

        /// <summary>
        /// Gets current core configuration.
        /// </summary>
        [Browsable(false)]
        public GraphicsCoreConfiguration CoreConfiguration
        {
            get { return m_coreConfig; }
        }
    }
}