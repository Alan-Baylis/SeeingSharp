﻿#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
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
using Xunit;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

using GDI = System.Drawing;

namespace SeeingSharp.Tests.Rendering
{
    [Collection("Rendering_SeeingSharp")]
    public class Drawing2DTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Multimedia Drawing 2D";

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public void Collision_Ellipse_to_Ellipse()
        {
            EllipseGeometryResource ellipseGeometry01 = new EllipseGeometryResource(
                new Vector2(50, 50), 10f, 10f);
            EllipseGeometryResource ellipseGeometry02 = new EllipseGeometryResource(
                new Vector2(50, 80), 10f, 10f);
            EllipseGeometryResource ellipseGeometry03 = new EllipseGeometryResource(
                new Vector2(50, 70), 10f, 11f);
            try
            {
                Assert.False(ellipseGeometry01.IntersectsWith(ellipseGeometry02));
                Assert.True(ellipseGeometry03.IntersectsWith(ellipseGeometry02));
                Assert.True(ellipseGeometry02.IntersectsWith(ellipseGeometry03));
            }
            finally
            {
                CommonTools.SafeDispose(ref ellipseGeometry01);
                CommonTools.SafeDispose(ref ellipseGeometry02);
                CommonTools.SafeDispose(ref ellipseGeometry03);
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public void Collision_Ellipse_to_Polygon()
        {
            EllipseGeometryResource ellipseGeometry01 = new EllipseGeometryResource(
                new Vector2(50, 50), 10f, 10f);
            EllipseGeometryResource ellipseGeometry02 = new EllipseGeometryResource(
                new Vector2(50, 80), 10f, 10f);

            PolygonGeometryResource polygonGeometry = new PolygonGeometryResource(new Polygon2D(new Vector2[]
            {
                new Vector2(55f, 50f),
                new Vector2(60f, 50f),
                new Vector2(55f, 55f)
            }));

            try
            {
                Assert.True(ellipseGeometry01.IntersectsWith(polygonGeometry));
                Assert.False(ellipseGeometry02.IntersectsWith(polygonGeometry));
            }
            finally
            {
                CommonTools.SafeDispose(ref ellipseGeometry01);
                CommonTools.SafeDispose(ref ellipseGeometry02);
                CommonTools.SafeDispose(ref polygonGeometry);
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleText_SimpleSingleColor()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (SolidBrushResource solidBrush = new SolidBrushResource(Color4.RedColor))
            using (TextFormatResource textFormat = new TextFormatResource("Arial", 70))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.DrawText(
                        string.Format("Just a dummy text ;){0}Just a dummy text ;)", Environment.NewLine),
                        textFormat,
                        new RectangleF(10, 10, 512, 512),
                        solidBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleText_SingleColor);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_Solid()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (SolidBrushResource solidBrush = new SolidBrushResource(Color4.Gray))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 512, 512), 30, 30,
                        solidBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleRoundedRectFilled);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleGeometry()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            Polygon2D polygon = new Polygon2D(new Vector2[]
            {
                new Vector2(10, 10),
                new Vector2(900, 100),
                new Vector2(800, 924),
                new Vector2(50, 1014),
                new Vector2(10, 10)
            });

            using (SolidBrushResource solidBrush = new SolidBrushResource(Color4.LightGray))
            using (SolidBrushResource solidBrushBorder = new SolidBrushResource(Color4.Gray))
            using (PolygonGeometryResource polygonGeometry = new PolygonGeometryResource(polygon))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.DrawGeometry(polygonGeometry, solidBrushBorder, 3f);
                    graphics.FillGeometry(polygonGeometry, solidBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleGeometry2D);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleGeometry_Ellipse()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (SolidBrushResource solidBrush = new SolidBrushResource(Color4.LightGray))
            using (SolidBrushResource solidBrushBorder = new SolidBrushResource(Color4.Gray))
            using (EllipseGeometryResource ellipseGeometry = new EllipseGeometryResource(new Vector2(512, 512), 400f, 300f))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.DrawGeometry(ellipseGeometry, solidBrushBorder, 3f);
                    graphics.FillGeometry(ellipseGeometry, solidBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleGeometry2D_Ellipse);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_LinearGradient()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (LinearGradientBrushResource gradientBrush = new LinearGradientBrushResource(
                new Vector2(0f, 0f),
                new Vector2(512f, 0f),
                new GradientStop[]
                {
                    new GradientStop(Color4.Gray, 0f),
                    new GradientStop(Color4.White, 0.6f),
                    new GradientStop(Color4.Black, 1f)
                },
                extendMode: ExtendMode.Mirror))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 900, 900), 30, 30,
                        gradientBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleRoundedRectFilled_LinearGradient);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_RadialGradient()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (RadialGradientBrushResource radialGradientBrush = new RadialGradientBrushResource(
                new Vector2(200f, 400f),
                new Vector2(0f, 0f),
                200f, 400f,
                new GradientStop[]
                {
                    new GradientStop(Color4.Gray, 0f),
                    new GradientStop(Color4.White, 0.6f),
                    new GradientStop(Color4.BlueColor, 1f)
                },
                extendMode: ExtendMode.Clamp))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 900, 900), 30, 30,
                        radialGradientBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleRoundedRectFilled_RadialGradient);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_Over3D()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (SolidBrushResource solidBrush = new SolidBrushResource(Color4.Gray.ChangeAlphaTo(0.5f)))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -5f);
                camera.Target = new Vector3(0f, 1f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                {
                    // Define object
                    NamedOrGenericKey geoResource = manipulator.AddResource<GeometryResource>(
                        () => new GeometryResource(new PalletStackType(
                            NamedOrGenericKey.Empty, 10)));
                    var newObject = manipulator.AddGeneric(geoResource);
                    newObject.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newObject.Scaling = new Vector3(2f, 2f, 2f);
                    newObject.Color = Color4.Goldenrod;
                    newObject.EnableShaderGeneratedBorder();
                });

                // Define 2D overlay
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 512, 512), 30, 30,
                        solidBrush);
                });

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_RoundedRectOver3D);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.True(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_DebugLayer()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (DebugDrawingLayer debugLayer = new DebugDrawingLayer())
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(debugLayer);
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_DebugDawingLayer);
                Assert.True(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleObject_D2D_Texture()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (SolidBrushResource solidBrush = new SolidBrushResource(Color4.Gray))
            using (TextFormatResource textFormat = new TextFormatResource("Arial", 36))
            using (SolidBrushResource textBrush = new SolidBrushResource(Color4.RedColor))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // 2D rendering is made here
                Custom2DDrawingLayer d2dDrawingLayer = new Custom2DDrawingLayer((graphics) =>
                {
                    RectangleF d2dRectangle = new RectangleF(10, 10, 236, 236);
                    graphics.Clear(Color4.LightBlue);
                    graphics.FillRoundedRectangle(
                        d2dRectangle, 30, 30,
                        solidBrush);

                    d2dRectangle.Inflate(-10, -10);
                    graphics.DrawText("Hello Direct2D!", textFormat, d2dRectangle, textBrush);
                });

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                {
                    var resD2DTexture = manipulator.AddResource<Direct2DTextureResource>(
                        () => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                    var resD2DMaterial = manipulator.AddSimpleColoredMaterial(resD2DTexture);
                    var geoResource = manipulator.AddResource<GeometryResource>(
                        () => new GeometryResource(new PalletType(
                            palletMaterial: NamedOrGenericKey.Empty,
                            contentMaterial: resD2DMaterial)));

                    GenericObject newObject = manipulator.AddGeneric(geoResource);
                    newObject.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newObject.Scaling = new Vector3(2f, 2f, 2f);
                });

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                bool isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, Properties.Resources.ReferenceImage_SimpleObject_D2DTexture);
                Assert.True(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.True(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleBitmap_WithTransparency()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (SolidBrushResource solidBrush = new SolidBrushResource(Color4.LightGray))
            using (StandardBitmapResource bitmap = new StandardBitmapResource(new AssemblyResourceLink(this.GetType(), "Ressources.Bitmaps.Logo.png")))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    graphics.FillRectangle(graphics.ScreenBounds, solidBrush);
                    graphics.DrawBitmap(bitmap, new Vector2(100f, 100f));
                });

                //await AsyncResourceLoader.Current.WaitForAllFinishedAsync();
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleBitmap_Transparency);
                Assert.True(diff < 0.02, "Difference to reference image is to big!");

            }
        }

        [Fact]
        [Trait("Category", TEST_CATEGORY)]
        public async Task Render_SimpleBitmap_Animated()
        {
            await UnitTestHelper.InitializeWithGrahicsAsync();

            using (StandardBitmapResource bitmap = new StandardBitmapResource(
                new AssemblyResourceLink(this.GetType(), "Ressources.Bitmaps.Boom.png"),
                frameCountX: 8, frameCountY: 8))
            using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync((graphics) =>
                {
                    // 2D rendering is made here
                    for (int loop = 0; loop < 8; loop++)
                    {
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 10f), frameIndex: 7);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 100f), frameIndex: 15);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 200f), frameIndex: 23);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 300f), frameIndex: 31);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 400f), frameIndex: 39);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 500f), frameIndex: 47);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 600f), frameIndex: 55);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 700f), frameIndex: 63);
                    }
                });

                //await AsyncResourceLoader.Current.WaitForAllFinishedAsync();
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //screenshot.DumpToDesktop("Blub.png");

                // Calculate and check difference
                float diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, Properties.Resources.ReferenceImage_SimpleBitmap_Animated);
                Assert.True(diff < 0.02, "Difference to reference image is to big!");

            }
        }
    }
}
