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
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Linq;
using SeeingSharp.Util;

#if UNIVERSAL
using Windows.Storage;
#endif

namespace RK2048.Data
{
    [XmlType]
    public class GameDataContainer
    {
        #region Constants
        private const string GAME_DATA_FILE_NAME = "GameData.{0}.xml";
        private const int GAME_SCORE_MAX = 20;
        #endregion

        #region Container properties
        private string m_gameName;
        #endregion

        #region Data collections
        private ObservableCollection<GameScore> m_gameScores;
        #endregion

        public GameDataContainer()
            : this(string.Empty)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameDataContainer" /> class.
        /// </summary>
        public GameDataContainer(string gameName)
        {
            m_gameName = gameName;
            m_gameScores = new ObservableCollection<GameScore>();

            // Observe score collection
            m_gameScores.CollectionChanged += (sender, eArgs) =>
            {
                    // Change score index
                    int actScoreIndex = 1;
                foreach (var actItem in m_gameScores)
                {
                    actItem.ScoreIndex = actScoreIndex;
                    actScoreIndex++;
                }
            };
        }

        /// <summary>
        /// Posts a new score value.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <param name="totalScore">The value to be posted.</param>
        public GameScore TryPostNewScore(string playerName, int totalScore)
        {
            GameScore newScore = new GameScore(playerName, totalScore);

            // Try to insert the score on correct location
            bool added = false;
            for (int loop = 0; loop < m_gameScores.Count; loop++)
            {
                if (m_gameScores[loop].TotalScore < totalScore)
                {
                    m_gameScores.Insert(loop, newScore);
                    added = true;
                    break;
                }
            }

            // Add the value to the end 
            if (!added && (m_gameScores.Count < GAME_SCORE_MAX))
            {
                m_gameScores.Add(newScore);
                added = true;
            }

            // Cut out last entries if collection is too big
            while (m_gameScores.Count > GAME_SCORE_MAX)
            {
                m_gameScores.RemoveAt(GAME_SCORE_MAX);
            }

            return added ? newScore : null;
        }

        /// <summary>
        /// Saves this data container to the roaming folder.
        /// </summary>
        public async Task SaveToRoamingFolder()
        {
#if UNIVERSAL
                StorageFolder sourceFolder = ApplicationData.Current.RoamingFolder;

                //Save updated highscore to file again
                StorageFile highscoreFileOut = await sourceFolder.CreateFileAsync(string.Format(GAME_DATA_FILE_NAME, m_gameName), CreationCollisionOption.ReplaceExisting);
                await CommonTools.SerializeToXmlFile(highscoreFileOut, this);
#elif DESKTOP
                // Get the file name of the highscore file
                string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingFolder, string.Format(GAME_DATA_FILE_NAME, m_gameName));

                // Save updated highscore to file again
                await CommonTools.SerializeToXmlFileAsync(filePath, this);
#endif
        }

        /// <summary>
        /// Loads the GameDataContainer from the roaming folder.
        /// </summary>
        public static async Task<GameDataContainer> LoadFromRoamingFolderAsync(string gameName)
        {
            GameDataContainer loadedContainer = null;

#if UNIVERSAL
                StorageFolder sourceFolder = ApplicationData.Current.RoamingFolder;

                //Read the highscore file
                StorageFile highscoreFile = await sourceFolder.GetOrReturnNull(string.Format(GAME_DATA_FILE_NAME, gameName));
                if (highscoreFile == null)
                {
                    loadedContainer = new GameDataContainer(gameName);
                }
                else
                {
                    try
                    {
                        //Try to load current score from roaming file
                        GameDataContainer highScore = await CommonTools.DeserializeFromXmlFile<GameDataContainer>(highscoreFile);
                        if (highScore == null) { highScore = new GameDataContainer(gameName); }
                        loadedContainer = highScore;
                    }
                    catch (Exception)
                    {
                        //Any exception occurred while deserializing
                        loadedContainer = new GameDataContainer(gameName);
                    }

                }

#elif DESKTOP
                // Get the file name of the highscore file
                string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingFolder, string.Format(GAME_DATA_FILE_NAME, gameName));

                if(!File.Exists(filePath))
                {
                    loadedContainer = new GameDataContainer(gameName);
                }
                else
                {
                    try
                    {
                        //Try to load current score from roaming file
                        GameDataContainer highScore = await CommonTools.DeserializeFromXmlFileAsync<GameDataContainer>(filePath);
                        if (highScore == null) { highScore = new GameDataContainer(gameName); }
                        loadedContainer = highScore;
                    }
                    catch(Exception)
                    {
                        //Any exception occurred while deserializing
                        loadedContainer = new GameDataContainer(gameName);
                    }
                }
#endif

            // Ensure that the game-name member is set correctly
            if (loadedContainer != null)
            {
                loadedContainer.m_gameName = gameName;
            }

            return loadedContainer;
        }

        /// <summary>
        /// Gets a list containing all game scores.
        /// </summary>
        [XmlElement("GameScore")]
        public ObservableCollection<GameScore> GameScores
        {
            get { return m_gameScores; }
        }
    }
}