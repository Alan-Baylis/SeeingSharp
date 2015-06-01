﻿using SeeingSharp.Infrastructure;
using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RKWebcamCapture
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Default initializations
            SeeingSharpApplication.InitializeAsync(
                Assembly.GetExecutingAssembly(),
                new Assembly[]{
                    typeof(GraphicsCore).Assembly,
                },
                new string[0]).Wait();
            GraphicsCore.Initialize(TargetHardware.Direct3D11, false);

            // Run the application
            MainWindow mainWindow = new MainWindow();
            SeeingSharpApplication.Current.InitializeUIEnvironment();
            Application.Run(mainWindow);
        }
    }
}
