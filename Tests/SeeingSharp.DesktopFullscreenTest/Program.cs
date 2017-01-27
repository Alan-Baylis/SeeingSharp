﻿using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Multimedia.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeeingSharp.DesktopFullscreenTest
{
    static class Program
    {
        private static FullscreenRenderTarget s_renderTarget;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Idle += OnApplication_FirstIdle;
            Application.Run();
        }

        private static async void OnApplication_FirstIdle(object sender, EventArgs e)
        {
            Application.Idle -= OnApplication_FirstIdle;

            await GraphicsCore.InitializeDefaultAsync();

            s_renderTarget = new FullscreenRenderTarget(
                GraphicsCore.Current.DefaultOutput);
            s_renderTarget.ClearColor = Color4.CornflowerBlue;
            s_renderTarget.RenderLoop.SceneComponents.Add(new ExitComponent());
            s_renderTarget.RenderLoop.SceneComponents.Add(new GradientBackgroundComponent());
            s_renderTarget.RenderLoop.SceneComponents.Add(new SimpleCenteredCubeComponent());
            s_renderTarget.RenderLoop.SceneComponents.Add(new FocusedPointCameraComponent());
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ExitComponent : SceneComponent
        {
            public ExitComponent()
            {

            }

            protected override void Attach(SceneManipulator manipulator, ViewInformation correspondingView)
            {
                
            }

            protected override void Detach(SceneManipulator manipulator, ViewInformation correspondingView)
            {
                
            }

            protected override void Update(SceneRelatedUpdateState updateState, ViewInformation correspondingView)
            {
                base.Update(updateState, correspondingView);

                foreach(var actInputFrame in updateState.InputFrames)
                {
                    if(actInputFrame.DefaultKeyboard.IsKeyDown(WinVirtualKey.Escape))
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }
    }
}