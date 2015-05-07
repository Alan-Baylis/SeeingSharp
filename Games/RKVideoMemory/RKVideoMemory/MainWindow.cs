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
#endregion

using SeeingSharp.Infrastructure;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using RKVideoMemory.Data;
using RKVideoMemory.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RKVideoMemory
{
    public partial class MainWindow : Form
    {
        private GameCore m_game;
        private bool m_onTickProcessing;
        private List<SceneObject> m_objectsBelowCursor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            m_objectsBelowCursor = new List<SceneObject>();
        }

        /// <summary>
        /// Generic update logic for this dialog.
        /// </summary>
        private void UpdateDialogStates()
        {
            this.Enabled =
                (m_game != null) &&
                (m_game.IsInitialized);
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!SeeingSharpApplication.IsInitialized) { return; }

            // Subscribe all handlers to the UIMessenger
            SeeingSharpApplication.Current.UIMessenger
                .SubscribeAllOnControl(this);

            // Associate game logic to UI
            m_game = new GameCore();
            m_ctrlRenderer.RenderLoop.Camera = m_game.Camera;
            m_ctrlRenderer.RenderLoop.SetScene(m_game.Scene);

            // Initialize game logic
            await m_game.InitializeAsync();
        }

        private void OnMessage_Received(GameInitializedMessage message)
        {
            this.UpdateDialogStates();
        }

        private void OnMessage_Received(LevelLoadedMessage message)
        {
            this.UpdateDialogStates();
        }

        private void OnMessage_Received(LevelUnloadedMessage message)
        {
            this.UpdateDialogStates();
        }

        /// <summary>
        /// Called when the game requests to display a video.
        /// </summary>
        private async void OnMessage_Received(PlayMovieRequestMessage message)
        {
            m_ctrlRenderer.DiscardRendering = true;
            try
            {
                // Perform rendering here
                await m_mediaPlayer.OpenAndShowVideoFileAsync(message.VideoLink);
            }
            catch(Exception)
            {
                m_ctrlRenderer.DiscardRendering = false;
                throw;
            }
        }

        /// <summary>
        /// Called when video playing is completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnMediaPlayer_VideoFinished(object sender, EventArgs e)
        {
            m_ctrlRenderer.DiscardRendering = false;
        }

        private void OnCtrlRenderer_MouseClick(object sender, MouseEventArgs e)
        {
            if (m_objectsBelowCursor.Count > 0)
            {
                SeeingSharpApplication.Current.UIMessenger.Publish(
                    new ObjectsClickedMessage(m_objectsBelowCursor.ToList()));
            }
        }

        private void OnTimerTrigger_Tick(object sender, EventArgs e)
        {
            SeeingSharpApplication.Current.UIMessenger.Publish<GameTriggerMessage>();
        }

        private async void OnTimerPicking_Tick(object sender, EventArgs e)
        {
            if (m_onTickProcessing) { return; }

            m_onTickProcessing = true;
            try
            {
                // Perform simple picking test
                System.Drawing.Point cursorPosition = this.PointToClient(Cursor.Position);
                List<SceneObject> objectsBelowCursor = await m_ctrlRenderer.RenderLoop.PickObjectAsync(
                    new SeeingSharp.Point(cursorPosition.X, cursorPosition.Y),
                    new PickingOptions());
                if (this.IsDisposed) { return; }

                // Look, what is new and what is old
                List<SceneObject> removedObjects = new List<SceneObject>(m_objectsBelowCursor.Count);
                List<SceneObject> addedObjects = new List<SceneObject>(objectsBelowCursor.Count);
                foreach(var actPickedObject in m_objectsBelowCursor)
                {
                    if(!objectsBelowCursor.Contains(actPickedObject))
                    {
                        removedObjects.Add(actPickedObject);
                    }
                }
                foreach(var actObjectBelowCurser in objectsBelowCursor)
                {
                    if(!m_objectsBelowCursor.Contains(actObjectBelowCurser))
                    {
                        addedObjects.Add(actObjectBelowCurser);
                    }
                }

                // Update local collection
                foreach (var actRemovedObject in removedObjects) { m_objectsBelowCursor.Remove(actRemovedObject); }
                foreach (var actAddedObject in addedObjects) { m_objectsBelowCursor.Add(actAddedObject); }

                // Notify changes to game system
                HoveredObjectsChangedMessage hoveredChangedMessage = new HoveredObjectsChangedMessage(
                    removedObjects, addedObjects);
                SeeingSharpApplication.Current.UIMessenger.Publish(hoveredChangedMessage);
            }
            finally
            {
                m_onTickProcessing = false;
            }
        }

        private async void OnCmdLoadLevel_Click(object sender, EventArgs e)
        {
            if(m_dlgOpenDir.ShowDialog(this) == DialogResult.OK)
            {
                await m_game.LoadLevelAsync(m_dlgOpenDir.SelectedPath);
            }
        }
    }
}
