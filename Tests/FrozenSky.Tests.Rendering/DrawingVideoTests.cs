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
*/
#endregion
using FrozenSky.Multimedia.Core;
using FrozenSky.Multimedia.Drawing3D;
using FrozenSky.Multimedia.DrawingVideo;
using FrozenSky.Multimedia.Objects;
using FrozenSky.Multimedia.Views;
using FrozenSky.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using GDI = System.Drawing;

namespace FrozenSky.Tests.Rendering
{
    [Collection("Rendering_FrozenSky")]
    public class DrawingVideoTests
    {
        public const string TEST_CATEGORY = "FrozenSky Multimedia Drawing Video";

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task RenderSimple_FullImages()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            // Configure video rendering
            string videoBasePath = Path.Combine(Environment.CurrentDirectory, "VideoImages");
            try
            {
                FullImageVideoWriter videoWriter = new FullImageVideoWriter();
                videoWriter.TargetDirectory = videoBasePath;

                // Perform video rendering
                await RenderSimple_Generic(
                    videoWriter, 4,
                    doAnimate: false);

                // Check results
                int imageCount = 0;
                foreach (string actFileName in Directory.GetFiles(videoBasePath))
                {
                    imageCount++;
                    using (GDI.Bitmap actBitmap = (GDI.Bitmap)GDI.Bitmap.FromFile(actFileName))
                    {
                        float diff = BitmapComparison.CalculatePercentageDifference(
                            actBitmap, Properties.Resources.ReferenceImage_SimpleObject);
                        Assert.True(diff < 0.05, "Difference to reference image is to big!");
                    }
                }
                Assert.True(imageCount >= 3);
                Assert.True(imageCount <= 6);
            }
            finally
            {
                if (Directory.Exists(videoBasePath))
                {
                    Directory.Delete(videoBasePath, true);
                }
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task RenderSimple_WmvVideo()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            // Configure video rendering
            string videoBasePath = Path.Combine(Environment.CurrentDirectory, "Videos");
            try
            {
                WmvVideoWriter videoWriter = new WmvVideoWriter();
                videoWriter.TargetDirectory = videoBasePath;
                videoWriter.FileNameTemplate = "Video_{0}.wmv";
                videoWriter.Bitrate = 1500;

                // Perform video rendering
                await RenderSimple_Generic(videoWriter, 100);

                // Check results
                Assert.False(string.IsNullOrEmpty(videoWriter.LastGeneratedFileName));
                Assert.True(File.Exists(videoWriter.LastGeneratedFileName));
            }
            finally
            {
                if (Directory.Exists(videoBasePath))
                {
                    Directory.Delete(videoBasePath, true);
                }
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task RenderSimple_Mp4Video()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            // Configure video rendering
            string videoBasePath = Path.Combine(Environment.CurrentDirectory, "Videos");
            try
            {
                Mp4VideoWriter videoWriter = new Mp4VideoWriter();
                videoWriter.TargetDirectory = videoBasePath;
                videoWriter.FileNameTemplate = "Video_{0}.mp4";
                videoWriter.Bitrate = 1500;

                // Perform video rendering
                await RenderSimple_Generic(videoWriter, 100);

                // Check results
                Assert.False(string.IsNullOrEmpty(videoWriter.LastGeneratedFileName));
                Assert.True(File.Exists(videoWriter.LastGeneratedFileName));
            }
            finally
            {
                if (Directory.Exists(videoBasePath))
                {
                    Directory.Delete(videoBasePath, true);
                }
            }
        }

        /// <summary>
        /// Base method used as common test scenario for all video renderers.
        /// </summary>
        /// <param name="videoWriter">The VideoWriter to be testet.</param>
        /// <param name="countFrames">Total count of frames to be rendered.</param>
        /// <param name="doAnimate">Execute animation during recording.</param>
        private async Task RenderSimple_Generic(FrozenSkyVideoWriter videoWriter, int countFrames, bool doAnimate = true)
        {
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                {
                    NamedOrGenericKey geoResource = manipulator.AddResource<GeometryResource>(
                        () => new GeometryResource(new PalletType()));

                    var newObject = manipulator.AddGeneric(geoResource);
                    newObject.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newObject.Scaling = new Vector3(2f, 2f, 2f);

                    if (doAnimate)
                    {
                        newObject.BuildAnimationSequence()
                            .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromMilliseconds(500.0))
                            .WaitFinished()
                            .RotateEulerAnglesTo(new Vector3(0f, 0, 0f), TimeSpan.FromMilliseconds(500.0))
                            .WaitFinished()
                            .ApplyAndRewind();
                    }
                });

                await memRenderTarget.RenderLoop.WaitForNextFinishedRenderAsync();

                // Start video rendering
                await memRenderTarget.RenderLoop.RegisterVideoWriterAsync(videoWriter);

                // Write about 100 frames to the video
                for (int loop = 0; loop < countFrames; loop++)
                {
                    await memRenderTarget.RenderLoop.WaitForNextFinishedRenderAsync();
                }

                // finish video rendering
                await memRenderTarget.RenderLoop.FinishVideoWriterAsync(videoWriter);
            }

            // Make shure that all the renderloop is correctly disposed
            Assert.True(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }
    }
}