﻿#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# Tools. More info at 
     - https://github.com/RolandKoenig/SeeingSharp/tree/master/Tools (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
	This program is distributed under the terms of the Microsoft Public License (Ms-PL)-
	More info at https://msdn.microsoft.com/en-us/library/ff647676.aspx
*/
#endregion
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharpModelViewer
{
    public class LoadedFileViewModel : ViewModelBase
    {
        #region Environment
        private Scene m_scene;
        #endregion

        #region Loaded data
        private ResourceLink m_currentFile;
        private ImportOptions m_currentImportOptions;
        #endregion Loaded data

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedFileViewModel"/> class.
        /// </summary>
        public LoadedFileViewModel(Scene scene)
        {
            scene.EnsureNotNull(nameof(scene));

            m_scene = scene;
        }

        /// <summary>
        /// Imports a new file by the given <see cref="ResourceLink"/>.
        /// </summary>
        /// <param name="resourceLink">The <see cref="ResourceLink"/> from which to load the resource.</param>
        public async Task ImportNewFileAsync(ResourceLink resourceLink)
        {
            m_currentFile = resourceLink;
            m_currentImportOptions = GraphicsCore.Current.ImportersAndExporters.CreateImportOptions(m_currentFile);
            base.RaisePropertyChanged(nameof(CurrentFile));
            base.RaisePropertyChanged(nameof(CurrentFileForStatusBar));
            base.RaisePropertyChanged(nameof(CurrentImportOptions));

            await m_scene.ImportAsync(m_currentFile, m_currentImportOptions);

            base.Messenger.Publish<NewModelLoadedMessage>();
        }

        public async Task ReloadCurrentFileAsync()
        {
            m_currentFile.EnsureNotNull(nameof(m_currentFile));
            m_currentImportOptions.EnsureNotNull(nameof(m_currentImportOptions));

            await CloseAsync(
                clearCurrentFileInfo: false);

            await m_scene.ImportAsync(m_currentFile, m_currentImportOptions);
        }

        public async Task CloseAsync(bool clearCurrentFileInfo = true)
        {
            await m_scene.ManipulateSceneAsync(manipulator => manipulator.ClearLayer(Scene.DEFAULT_LAYER_NAME));

            if (clearCurrentFileInfo)
            {
                m_currentFile = null;
                m_currentImportOptions = null;
                RaisePropertyChanged(nameof(CurrentFile));
                RaisePropertyChanged(nameof(CurrentFileForStatusBar));
                RaisePropertyChanged(nameof(CurrentImportOptions));
            }
        }

        public ResourceLink CurrentFile
        {
            get { return m_currentFile; }
        }

        public string CurrentFileForStatusBar
        {
            get
            {
                DesktopFileSystemResourceLink fileResource = this.CurrentFile as DesktopFileSystemResourceLink;
                if(fileResource == null) { return "-"; }
                else
                {
                    string filePath = fileResource.FilePath;
                    if(filePath.Length > 50) { return $"...{filePath.Substring(filePath.Length - 45)}"; }
                    else { return filePath; }
                }
            }
        }

        public ImportOptions CurrentImportOptions
        {
            get { return m_currentImportOptions; }
        }
    }
}
