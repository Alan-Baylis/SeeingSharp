﻿#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SeeingSharp.Multimedia.Objects;

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// The default structure for sending vertex data to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct StandardVertex
    {
        #region Constants
        public static readonly int Size = Marshal.SizeOf<StandardVertex>();
        public static readonly D3D11.InputElement[] InputElements = new D3D11.InputElement[]
        {
            new D3D11.InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0),
            new D3D11.InputElement("NORMAL", 0, DXGI.Format.R32G32B32_Float, 12, 0),
            new D3D11.InputElement("TANGENT", 0, DXGI.Format.R32G32B32_Float, 24, 0),
            new D3D11.InputElement("BINORMAL", 0, DXGI.Format.R32G32B32_Float, 36, 0),
            new D3D11.InputElement("COLOR", 0, DXGI.Format.R8G8B8A8_UNorm, 48, 0),
            new D3D11.InputElement("TEXCOORD", 0, DXGI.Format.R32G32_Float, 52, 0),
            new D3D11.InputElement("TEXCOORD", 1, DXGI.Format.R32_Float, 60, 0)
        };
        #endregion

        #region  All vertex elements
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Binormal;
        public int Color;
        public Vector2 Texture;
        public float TextureFactor;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public StandardVertex(Vector3 position, int color)
        {
            this.Position = position;
            this.Normal = new Vector3(0, 1, 0);
            this.Texture = new Vector2(0f, 0f);
            this.Color = color;
            this.Tangent = Vector3.Zero;
            this.Binormal = Vector3.Zero;
            this.TextureFactor = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texCoord">The texture coordinate.</param>
        public StandardVertex(Vector3 position, Vector2 texCoord)
        {
            this.Position = position;
            this.Normal = new Vector3();
            this.Texture = texCoord;
            this.Color = 0;
            this.Tangent = Vector3.Zero;
            this.Binormal = Vector3.Zero;
            this.TextureFactor = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texCoord">The texture coordinate.</param>
        /// <param name="normal">The normal.</param>
        public StandardVertex(Vector3 position, Vector2 texCoord, Vector3 normal)
        {
            this.Position = position;
            this.Normal = normal;
            this.Texture = texCoord;
            this.Color = 0;
            this.Tangent = Vector3.Zero;
            this.Binormal = Vector3.Zero;
            this.TextureFactor = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="vertex">Source vertex data.</param>
        public StandardVertex(Vertex vertex)
        {
            this.Position = vertex.Position;
            this.Normal = vertex.Normal;
            this.Texture = vertex.TexCoord;
            this.Color = vertex.Color.ToRgba();
            this.Tangent = vertex.Tangent;
            this.Binormal = vertex.Binormal;
            this.TextureFactor = vertex.TextureFactor;
        }

        /// <summary>
        /// Gets an array of StandardVertices from the given VertexStructure object.
        /// </summary>
        /// <param name="source">The VertexStructure object.</param>
        public static StandardVertex[] FromVertexStructure(VertexStructure source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            int vertexCount = source.CountVertices;

            //Create result array
            StandardVertex[] result = new StandardVertex[vertexCount];
            for (int loop = 0; loop < source.CountVertices; loop++)
            {
                Vertex vertex = source.Vertices[loop];
                result[loop] = new StandardVertex(vertex);
            }

            return result;
        }

        /// <summary>
        /// Gets an array of StandardVertices from the given VertexStructure objects.
        /// </summary>
        public static StandardVertex[] FromVertexStructure(VertexStructure[] structures)
        {
            if (structures == null) { throw new ArgumentNullException("structures"); }

            //Get total vertex count
            int vertexCount = 0;
            for (int loop = 0; loop < structures.Length; loop++)
            {
                vertexCount += structures[loop].CountVertices;
            }

            //create result array
            StandardVertex[] result = new StandardVertex[vertexCount];
            int actVertexPos = 0;
            for (int loop = 0; loop < structures.Length; loop++)
            {
                VertexStructure actStructure = structures[loop];
                int structureVertexCount = actStructure.CountVertices;
                for (int innerLoop = 0; innerLoop < structureVertexCount; innerLoop++)
                {
                    result[actVertexPos] = new StandardVertex(actStructure.Vertices[innerLoop]);
                    actVertexPos++;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an array containing all vertices needed to display a list generated by given function.
        /// </summary>
        /// <param name="lineColor">Color of the line.</param>
        /// <param name="lineListCreator">Function that creates each line.</param>
        public static StandardVertex[] FromLineList(Color4 lineColor, System.Func<List<Vector3>> lineListCreator)
        {
            return FromLineList(lineColor, lineListCreator());
        }

        /// <summary>
        /// Creates an array containing all vertices needed to display the given list of lines.
        /// </summary>
        /// <param name="lineColor">Color of the line.</param>
        /// <param name="lineList">List containing the lines.</param>
        public static StandardVertex[] FromLineList(Color4 lineColor, List<Vector3> lineList)
        {
            return FromLineList(lineColor, lineList.ToArray());
        }

        /// <summary>
        /// Creates an array containing all vertices needed to display the given list of lines.
        /// </summary>
        /// <param name="lineColor">Color of the line.</param>
        /// <param name="lineList">List containing the lines.</param>
        public static StandardVertex[] FromLineList(Color4 lineColor, params Vector3[] lineList)
        {
            if ((lineList == null) || (lineList.Length == 0)) { return new StandardVertex[0]; }

            StandardVertex[] result = new StandardVertex[lineList.Length];
            for (int loop = 0; loop < lineList.Length; loop++)
            {
                StandardVertex actVertex = new StandardVertex(lineList[loop], lineColor.ToRgba());
                result[loop] = actVertex;
            }
            return result;
        }
    }
}