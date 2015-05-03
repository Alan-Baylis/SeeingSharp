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

using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System.IO;


namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class ShaderResource : Resource
    {
        // Generic members
        private string m_shaderProfile;
        private byte[] m_shaderBytecode;
        private ResourceLink m_resourceLink;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="ShaderResource"/> class.
        ///// </summary>
        ///// <param name="name">The name of the resource.</param>
        ///// <param name="shaderProfile">Shader profile used for compilation.</param>
        ///// <param name="filePath">Path to shader file.</param>
        //protected ShaderResource(string name, string shaderProfile, string filePath)
        //    : base(name)
        //{
        //    m_precompiled = false;
        //    m_shaderProfile = shaderProfile;
        //    m_filePath = filePath;
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResource"/> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compilation.</param>
        /// <param name="resourceLink">The source of the resource.</param>
        protected ShaderResource(string shaderProfile, ResourceLink resourceLink)
        {
            m_shaderProfile = shaderProfile;
            m_resourceLink = resourceLink;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            //Load the shader itself
            if (m_shaderBytecode == null)
            {
                using (Stream inStream = m_resourceLink.OpenInputStream())
                {
                    m_shaderBytecode = inStream.ReadAllBytes();
                }
            }

            LoadShader(device, m_shaderBytecode);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            UnloadShader();
        }

        /// <summary>
        /// Loads a shader using the given bytecode.
        /// </summary>
        /// <param name="inStream">A reading stream to the shader's bytecode.</param>
        protected internal abstract void LoadShader(EngineDevice device, byte[] shaderBytecode);

        /// <summary>
        /// Unloads the shader.
        /// </summary>
        protected internal abstract void UnloadShader();

        /// <summary>
        /// Gets the shader's raw bytecode.
        /// </summary>
        public byte[] ShaderBytecode
        {
            get { return m_shaderBytecode; }
        }
    }
}