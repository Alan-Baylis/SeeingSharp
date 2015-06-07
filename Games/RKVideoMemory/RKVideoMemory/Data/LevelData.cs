﻿#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it.
    More info at
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
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
#endregion License information (SeeingSharp and all based games/applications)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Util;

namespace RKVideoMemory.Data
{
    public class LevelData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LevelData"/> class.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        private LevelData(string directoryName)
        {
            // Search all main textures
            this.MainTextures = new MainTextureData(directoryName);

            // Handle tilemap file
            string tilemapPath = Path.Combine(directoryName, Constants.TILEMAP_FILENAME);
            if (File.Exists(tilemapPath))
            {
                this.Tilemap = TilemapData.FromFile(tilemapPath);
            }
            else
            {
                this.Tilemap = new TilemapData();
            }

            // Set reference to the main icon
            string appIconPath = Path.Combine(directoryName, Constants.APPICON_FILENAME);
            if(File.Exists(appIconPath))
            {
                this.AppIconPath = appIconPath;
            }

            // Load all screens
            IEnumerable<string> screenDirectories =
                from actScreenDirectory in Directory.GetDirectories(directoryName)
                orderby Path.GetFileName(actScreenDirectory)
                select actScreenDirectory;
            this.Screens = new List<ScreenData>();
            foreach(string actScreenDirectory in screenDirectories)
            {
                ScreenData actScreen = new ScreenData(actScreenDirectory);
                if(actScreen.MemoryPairs.Count > 0)
                {
                    this.Screens.Add(actScreen);
                }
            }
        }

        /// <summary>
        /// Loads the level from the given directory.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        public static async Task<LevelData> FromDirectoryAsync(string directoryName)
        {
            LevelData result = null;
            await Task.Factory.StartNew(() => result = new LevelData(directoryName));
            return result;
        }

        public string AppIconPath
        {
            get;
            private set;
        }

        public MainTextureData MainTextures
        {
            get;
            private set;
        }

        public TilemapData Tilemap
        {
            get;
            private set;
        }

        public List<ScreenData> Screens
        {
            get;
            private set;
        }
    }
}