﻿#region License information (FrozenSky and all based games/applications)
/*
    FrozenSky and all games/applications based on it (more info at http://www.rolandk.de/wp)
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

	********************************************************************************
    Additional permission under GNU GPL version 3 section 7

    If you modify this Program, or any covered work, by linking or combining it with 
	DirectX (or a modified version of that library), containing parts covered by the 
	terms of [name of library's license], the licensors of this Program grant you additional 
	permission to convey the resulting work. {Corresponding Source for a non-source form of 
	such a combination shall include the source code for the parts of DirectX used 
	as well as that of the covered work.}
*/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrozenSky.Multimedia.Core;
using FrozenSky.Util;

// Some namespace mappings
using MF = SharpDX.MediaFoundation;

namespace FrozenSky.Multimedia.DrawingVideo
{
    /// <summary>
    /// Base class for interacting with WindowsMediaFoundation.
    /// </summary>
    public abstract class MediaFoundationVideoWriter : FrozenSkyVideoWriter
    {
        private static readonly Guid VIDEO_INPUT_FORMAT = MFVideoFormats.FORMAT_RBG32;

        // Configuration
        #region
        private int m_bitrate;
        private int m_framerate;
        #endregion

        // Resources for MediaFoundation video rendering
        #region
        private MF.SinkWriter m_sinkWriter;
        private Size2 m_videoPixelSize;
        private int m_frameIndex;
        private int m_streamIndex;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFoundationVideoWriter"/> class.
        /// </summary>
        public MediaFoundationVideoWriter()
        {
            m_bitrate = 1500000;
            m_framerate = 25;
        }

        /// <summary>
        /// Starts rendering to the target.
        /// </summary>
        /// <param name="videoPixelSize">The pixel size of the video.</param>
        protected override void StartRenderingInternal(Size2 videoPixelSize)
        {
            m_sinkWriter = MF.MediaFactory.CreateSinkWriterFromURL(
                base.GetNextFileName(), 
                IntPtr.Zero, null);
            m_videoPixelSize = videoPixelSize;

            CreateMediaTarget(m_sinkWriter, m_videoPixelSize, out m_streamIndex);

            // Configure input
            using (MF.MediaType mediaTypeIn = new MF.MediaType())
            {
                mediaTypeIn.Set<Guid>(MF.MediaTypeAttributeKeys.MajorType, MF.MediaTypeGuids.Video);
                mediaTypeIn.Set<Guid>(MF.MediaTypeAttributeKeys.Subtype, VIDEO_INPUT_FORMAT);
                mediaTypeIn.Set<int>(MF.MediaTypeAttributeKeys.InterlaceMode, (int)MF.VideoInterlaceMode.Progressive);
                mediaTypeIn.Set<long>(MF.MediaTypeAttributeKeys.FrameSize, MFHelper.GetMFLongEncodedInts(videoPixelSize.Width, videoPixelSize.Height));
                mediaTypeIn.Set<long>(MF.MediaTypeAttributeKeys.FrameRate, MFHelper.GetMFLongEncodedInts(m_framerate, 1));
                m_sinkWriter.SetInputMediaType(m_streamIndex, mediaTypeIn, null);
            }

            // Start writing the video file
            m_sinkWriter.BeginWriting();

            // Set initial frame index
            m_frameIndex = -1;
        }

        /// <summary>
        /// Draws the given frame to the video.
        /// </summary>
        /// <param name="device">The device on which the given framebuffer is created.</param>
        /// <param name="uploadedTexture">The texture which should be added to the video.</param>
        protected override void DrawFrameInternal(EngineDevice device, MemoryMappedTexture32bpp uploadedTexture)
        {
#if UNIVERSAL
            throw new NotImplementedException("Not implemented for WinRT currently!");
#else

            // Cancel here if the given texture has an invalid size
            if (m_videoPixelSize != new Size2(uploadedTexture.Width, uploadedTexture.Height)) { return; }

            m_frameIndex++;
            MF.MediaBuffer mediaBuffer = MF.MediaFactory.CreateMemoryBuffer((int)uploadedTexture.SizeInBytes);
            try
            {
                // Write all contents to the MediaBuffer for media foundation
                int cbMaxLength = 0;
                int cbCurrentLength = 0;
                IntPtr mediaBufferPointer = mediaBuffer.Lock(out cbMaxLength, out cbCurrentLength);
                try
                {

                    int stride = m_videoPixelSize.Width * 4;
                    if (this.FlipY)
                    {
                        for(int loop=0 ; loop<m_videoPixelSize.Height; loop++)
                        {
                            MF.MediaFactory.CopyImage(
                                mediaBufferPointer + (m_videoPixelSize.Height - (1 + loop)) * stride,
                                stride,
                                uploadedTexture.Pointer + loop * stride,
                                stride,
                                stride,
                                1);
                        }
                    }
                    else
                    {
                        MF.MediaFactory.CopyImage(
                            mediaBufferPointer,
                            stride,
                            uploadedTexture.Pointer,
                            stride,
                            stride,
                            m_videoPixelSize.Height);
                    }
                }
                finally
                {
                    mediaBuffer.Unlock();
                }
                mediaBuffer.CurrentLength = (int)uploadedTexture.SizeInBytes;

                // Create the sample (includes image and timing information)
                MF.Sample sample = MF.MediaFactory.CreateSample();
                try
                {
                    sample.AddBuffer(mediaBuffer);

                    long frameDuration = 10 * 1000 * 1000 / m_framerate;
                    sample.SampleTime = frameDuration * m_frameIndex;
                    sample.SampleDuration = frameDuration;

                    m_sinkWriter.WriteSample(m_streamIndex, sample);
                }
                finally
                {
                    GraphicsHelper.SafeDispose(ref sample);
                }
            }
            finally
            {
                GraphicsHelper.SafeDispose(ref mediaBuffer);
            }
#endif
        }

        /// <summary>
        /// Finishes rendering to the target (closes the video file).
        /// Video rendering can not be started again from this point on.
        /// </summary>
        protected override void FinishRenderingInternal()
        {
            m_sinkWriter.Finalize();

            GraphicsHelper.SafeDispose(ref m_sinkWriter);
        }

        /// <summary>
        /// Creates a media target.
        /// </summary>
        /// <param name="sinkWriter">The previously created SinkWriter.</param>
        /// <param name="videoPixelSize">The pixel size of the video.</param>
        /// <param name="streamIndex">The stream index for the new target.</param>
        protected abstract void CreateMediaTarget(MF.SinkWriter sinkWriter, Size2 videoPixelSize, out int streamIndex);

        /// <summary>
        /// Gets the total count of frames rendered.
        /// </summary>
        [Browsable(false)]
        public int CountRenderedFrames
        {
            get { return m_frameIndex; }
        }

        public int Bitrate
        {
            get { return m_bitrate / 1000; ; }
            set
            {
                base.CheckWhetherChangesAreValid();
                m_bitrate = value * 1000;
            }
        }

        public int Framerate
        {
            get { return m_framerate; }
            set
            {
                base.CheckWhetherChangesAreValid();
                m_framerate = value;
            }
        }

        /// <summary>
        /// Internal use: FlipY during rendering?
        /// </summary>
        [Browsable(false)]
        protected virtual bool FlipY
        {
            get { return false; }
        }
    }
}