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

using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class LineRenderResources : Resource
    {
        public static readonly NamedOrGenericKey RESOURCE_KEY = GraphicsCore.GetNextGenericResourceKey();

        private static readonly NamedOrGenericKey KEY_VERTEX_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey KEY_PIXEL_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();

        private VertexShaderResource m_vertexShader;
        private PixelShaderResource m_pixelShader;
        private TypeSafeConstantBufferResource<ConstantBufferData> m_constantBuffer;
        private D3D11.InputLayout m_inputLayout;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineRenderResources" /> class.
        /// </summary>
        public LineRenderResources()
        {

        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_vertexShader = resources.GetResourceAndEnsureLoaded(
                KEY_VERTEX_SHADER,
                () => GraphicsHelper.GetVertexShaderResource(device, "LineRendering", "LineVertexShader"));
            m_pixelShader = resources.GetResourceAndEnsureLoaded(
                KEY_PIXEL_SHADER,
                () => GraphicsHelper.GetPixelShaderResource(device, "LineRendering", "LinePixelShader"));
            m_constantBuffer = resources.GetResourceAndEnsureLoaded(
                KEY_CONSTANT_BUFFER,
                () => new TypeSafeConstantBufferResource<ConstantBufferData>());

            m_inputLayout = new D3D11.InputLayout(
                device.DeviceD3D11,
                m_vertexShader.ShaderBytecode,
                LineVertex.InputElements);
        }

        /// <summary>
        /// Renders all given lines with the given parameters.
        /// </summary>
        /// <param name="renderState">The render state to be used.</param>
        /// <param name="worldViewProj">Current world-view-project transformation.</param>
        /// <param name="lineColor">The color for the line.</param>
        /// <param name="lineVertexBuffer">The vertex buffer containing all line vertices.</param>
        /// <param name="vertexCount">Total count of vertices.</param>
        internal void RenderLines(RenderState renderState, Matrix worldViewProj, Color4 lineColor, D3D11.Buffer lineVertexBuffer, int vertexCount)
        {
            D3D11.DeviceContext deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            //Apply constant buffer data
            ConstantBufferData constantData = new ConstantBufferData();
            constantData.DiffuseColor = lineColor;
            constantData.WorldViewProj = worldViewProj;
            m_constantBuffer.SetData(deviceContext, constantData);

            //Apply vertex buffer and draw lines
            deviceContext.VertexShader.SetConstantBuffer(4, m_constantBuffer.ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(4, m_constantBuffer.ConstantBuffer);
            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(lineVertexBuffer, LineVertex.Size, 0));
            deviceContext.Draw(vertexCount, 0);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            GraphicsHelper.SafeDispose(ref m_inputLayout);

            m_vertexShader = null;
            m_pixelShader = null;
            m_constantBuffer = null;
        }

        /// <summary>
        /// Is the resource loaded correctly?
        /// </summary>
        public override bool IsLoaded
        {
            get { return m_pixelShader != null; }
        }

        /// <summary>
        /// Gets the vertex shader resource.
        /// </summary>
        public VertexShaderResource VertexShader
        {
            get { return m_vertexShader; }
        }

        /// <summary>
        /// Gets the pixel shader resource.
        /// </summary>
        public PixelShaderResource PixelShader
        {
            get { return m_pixelShader; }
        }

        /// <summary>
        /// Gets the constant buffer resource.
        /// </summary>
        public ConstantBufferResource ConstantBuffer
        {
            get { return m_constantBuffer; }
        }

        /// <summary>
        /// Gets the input layout for the vertex shader.
        /// </summary>
        internal D3D11.InputLayout InputLayout
        {
            get { return m_inputLayout; }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        [StructLayout(LayoutKind.Sequential)]
        private struct ConstantBufferData
        {
            public Matrix WorldViewProj;
            public Color4 DiffuseColor;
        }
    }
}