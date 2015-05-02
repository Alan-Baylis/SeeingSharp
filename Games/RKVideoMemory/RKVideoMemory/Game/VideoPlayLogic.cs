﻿#region License information (SeeingSharp and all based games/applications)
/*
    SeeingSharp and all games/applications based on it (more info at http://www.rolandk.de/wp)
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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RKVideoMemory.Game
{
    /// <summary>
    /// This component is responsible for playing videos when the player has uncovered a CardPair.
    /// </summary>
    public class VideoPlayLogic : SceneLogicalObject
    {
        /// <summary>
        /// Called when user has uncovered a CardPair.
        /// </summary>
        private async void OnMessage_Received(CardPairUncoveredByPlayerMessage message)
        {
            ResourceLink firstVideo =
                message.CardPair.PairData.ChildVideos.FirstOrDefault();
            if (firstVideo == null) { return; }

            await base.Scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create the layer (if necessary)
                if (!manipulator.ContainsLayer(Constants.GFX_LAYER_VIDEO_FOREGROUND))
                {
                    SceneLayer bgLayer = manipulator.AddLayer(Constants.GFX_LAYER_VIDEO_FOREGROUND);
                    bgLayer.ClearDepthBufferBefreRendering = true;
                    manipulator.SetLayerOrderID(
                        bgLayer,
                        Constants.GFX_LAYER_VIDEO_FOREGROUND_ORDERID);
                }

                // Load the texture painter
                var resBackgroundTexture = manipulator.AddResource(() => new VideoTextureResource(firstVideo));
                manipulator.Add(new TexturePainter(resBackgroundTexture), Constants.GFX_LAYER_VIDEO_FOREGROUND);
            });
        }
    }
}
