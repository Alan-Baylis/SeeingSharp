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

using SeeingSharp.Util;
using PropertyTools.DataAnnotations;
using System;
using System.Xml.Serialization;

namespace SeeingSharp.Multimedia.Core
{
    public class GraphicsViewConfiguration 
    {
        private const string CATEOGRY_COMMON = "Common";
        private const string CATEGORY_QUALITY = "Quality";
        private const string CATEGORY_DETAILS = "Details";

        private GraphicsDeviceConfiguration m_deviceConfig;

        private bool m_viewNeedsRefresh;

        private bool m_antialiasingEnabled;
        private AntialiasingQualityLevel m_antialiasingQuality;

        private float m_generatedColorGradientFactor;
        private float m_generatedBorderFactor;
        private float m_accentuationFactor;
        private float m_ambientFactor;
        private float m_lightPower;
        private float m_strongLightFactor;

        private bool m_overlay2DEnabled;

        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsViewConfiguration" /> class.
        /// </summary>
        /// <param name="deviceConfig">The device configuration object.</param>
        internal GraphicsViewConfiguration()
        {
            this.ShowTextures = true;
            this.AntialiasingEnabled = true;
            this.AntialiasingQuality = AntialiasingQualityLevel.Medium;

            // Define and execute reset action
            Action resetAction = () =>
            {
                ShowTexturesInternal = true;
                m_generatedBorderFactor = 1f;
                m_generatedColorGradientFactor = 1f;
                m_accentuationFactor = 0f;
                m_ambientFactor = 0.2f;
                m_lightPower = 0.8f;
                m_strongLightFactor = 1.5f;
                m_overlay2DEnabled = true;
            };
            resetAction();

            // Define commands
            ResetCommand = new DelegateCommand(resetAction);
        }

        [Category(CATEOGRY_COMMON)]
        public DelegateCommand ResetCommand
        {
            get;
            private set;
        }

        [Browsable(false)]
        internal bool ViewNeedsRefresh
        {
            get { return m_viewNeedsRefresh; }
            set { m_viewNeedsRefresh = value; }
        }

        /// <summary>
        /// Is wireframe rendering enabled?
        /// </summary>
        [Category(CATEGORY_QUALITY)]
        public bool WireframeEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Try to enable antialiasing?
        /// </summary>
        [Category(CATEGORY_QUALITY)]
        public bool AntialiasingEnabled
        {
            get { return m_antialiasingEnabled; }
            set
            {
                if (m_antialiasingEnabled != value)
                {
                    m_antialiasingEnabled = value;
                    m_viewNeedsRefresh = true;

                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The quality level for antialiasing (if antialiasing is enabled).
        /// </summary>
        [Category(CATEGORY_QUALITY)]
        [EnableBy("AntialiasingEnabled")]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get { return m_antialiasingQuality; }
            set
            {
                if (m_antialiasingQuality != value)
                {
                    m_antialiasingQuality = value;
                    m_viewNeedsRefresh = true;

                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Category(CATEGORY_DETAILS)]
        [Slidable(0.0, 1.0)]
        public float GeneratedColorGradientFactor
        {
            get { return m_generatedColorGradientFactor; }
            set
            {
                if (m_generatedColorGradientFactor != value)
                {
                    m_generatedColorGradientFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Category(CATEGORY_DETAILS)]
        [Slidable(0.0, 1.0)]
        public float GeneratedBorderFactor
        {
            get { return m_generatedBorderFactor; }
            set
            {
                if (m_generatedBorderFactor != value)
                {
                    m_generatedBorderFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Category(CATEGORY_DETAILS)]
        [Slidable(0.0, 1.0)]
        public float AccentuationFactor
        {
            get { return m_accentuationFactor; }
            set
            {
                if (m_accentuationFactor != value)
                {
                    m_accentuationFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Category(CATEGORY_DETAILS)]
        [Slidable(0.0, 1.0)]
        public float AmbientFactor
        {
            get { return m_ambientFactor; }
            set
            {
                if (m_ambientFactor != value)
                {
                    m_ambientFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Category(CATEGORY_DETAILS)]
        [Slidable(0.0, 1.0)]
        public float LightPower
        {
            get { return m_lightPower; }
            set
            {
                if (m_lightPower != value)
                {
                    m_lightPower = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Category(CATEGORY_DETAILS)]
        [Slidable(0.0, 5.0)]
        public float StrongLightFactor
        {
            get { return m_strongLightFactor; }
            set
            {
                if (m_strongLightFactor != value)
                {
                    m_strongLightFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Internal accessor for ShowTextures variable.
        /// </summary>
        internal bool ShowTexturesInternal;

        [XmlAttribute]
        [Category(CATEGORY_DETAILS)]
        public bool ShowTextures
        {
            get { return ShowTexturesInternal; }
            set
            {
                if (ShowTexturesInternal != value)
                {
                    ShowTexturesInternal = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Category(CATEGORY_DETAILS)]
        public bool Overlay2DEnabled
        {
            get { return m_overlay2DEnabled; }
            set
            {
                if(m_overlay2DEnabled != value)
                {
                    m_overlay2DEnabled = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets current device configuration.
        /// </summary>
        [Browsable(false)]
        public GraphicsDeviceConfiguration DeviceConfiguration
        {
            get { return m_deviceConfig; }
            internal set { m_deviceConfig = value; }
        }

        /// <summary>
        /// Gets current core configuration.
        /// </summary>
        [Browsable(false)]
        public GraphicsCoreConfiguration CoreConfiguration
        {
            get 
            {
                if (m_deviceConfig == null) { return null; }
                return m_deviceConfig.CoreConfiguration;
            }
        }
    }
}