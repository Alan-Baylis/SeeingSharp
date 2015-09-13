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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// A state object describing current mouse or pointer input.
    /// </summary>
    public class MouseOrPointerState : InputStateBase
    {
        public static readonly MouseOrPointerState Dummy = new MouseOrPointerState();
        private static readonly int BUTTON_COUNT = Enum.GetValues(typeof(MouseButton)).Length;

        #region Current state
        private Vector2 m_moveDistancePixel;
        private Vector2 m_positionPixel;
        private Vector2 m_screenSizePixel;
        private int m_wheelDelta;
        private bool[] m_buttonsHit;        // Only for one frame at true
        private bool[] m_buttonsDown;       // All following frames the mouse is down
        private bool[] m_buttonsUp;         // True on the frame when the button changes to up
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseOrPointerState"/> class.
        /// </summary>
        internal MouseOrPointerState()
        {
            int buttonCount = BUTTON_COUNT;
            if (buttonCount <= 0)
            {
                buttonCount = Enum.GetValues(typeof(MouseButton)).Length;
            }

            m_buttonsHit = new bool[buttonCount];
            m_buttonsDown = new bool[buttonCount];
            m_buttonsUp = new bool[buttonCount];
        }

        /// <summary>
        /// Returns true if the user pressed the given button in this frame.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonHit(MouseButton mouseButton)
        {
            return m_buttonsHit[(int)mouseButton];
        }

        /// <summary>
        /// Returns true if the user pressed this button before and still don't loosed it.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonDown(MouseButton mouseButton)
        {
            return m_buttonsDown[(int)mouseButton];
        }

        /// <summary>
        /// Returns true if the user loosed the given button in this frame.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonUp(MouseButton mouseButton)
        {
            return m_buttonsUp[(int)mouseButton];
        }

        /// <summary>
        /// Notifies the state of the mouse buttons.
        /// Called by input handler.
        /// </summary>
        internal void NotifyButtonStates(
            bool isLeftButtonDown,
            bool isMiddleButtonDown,
            bool isRightButtonDown,
            bool isExtended1ButtonDown,
            bool isExtended2ButtonDown)
        {
            bool[] buttonStates = new bool[]
            {
                isLeftButtonDown,
                isMiddleButtonDown,
                isRightButtonDown,
                isExtended1ButtonDown,
                isExtended2ButtonDown
            };

            // Check correct count of buttons
            buttonStates.EnsureCountEquals(BUTTON_COUNT, "buttonStates");

            // Update mouse states
            for(int loop=0; loop<buttonStates.Length; loop++)
            {
                bool isHitOrDown = m_buttonsHit[loop] | m_buttonsDown[loop];
                if(isHitOrDown == buttonStates[loop]) { continue; }

                if(buttonStates[loop])
                {
                    m_buttonsHit[loop] = true;
                    m_buttonsDown[loop] = true;
                }
                else
                {
                    m_buttonsHit[loop] = false;
                    m_buttonsDown[loop] = false;
                    m_buttonsUp[loop] = true;
                }
            }
        }

        /// <summary>
        /// Notifies some information about the mouse pointer.
        /// Called by input handler.
        /// </summary>
        internal void NotifyMouseLocation(Vector2 pixelPosition, Vector2 moveDistancePixel, Vector2 screenSizePixel)
        {
            m_positionPixel = pixelPosition;
            m_moveDistancePixel = moveDistancePixel;
            m_screenSizePixel = screenSizePixel;
        }

        /// <summary>
        /// Notifies the mouse wheel.
        /// Called by input handler.
        /// </summary>
        internal void NotifyMouseWheel(int wheelDelta)
        {
            m_wheelDelta += wheelDelta;
        }

        /// <summary>
        /// Copies this object and then resets it 
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected override InputStateBase CopyAndResetForUpdatePassInternal()
        {
            // Copy the object
            MouseOrPointerState result = new MouseOrPointerState();
            result.m_moveDistancePixel = m_moveDistancePixel;
            result.m_screenSizePixel = m_screenSizePixel;
            result.m_positionPixel = m_positionPixel;
            result.m_wheelDelta = m_wheelDelta;
            for (int loop = 0; loop < BUTTON_COUNT; loop++)
            {
                result.m_buttonsDown[loop] = m_buttonsDown[loop];
                result.m_buttonsHit[loop] = m_buttonsHit[loop];
                result.m_buttonsUp[loop] = m_buttonsUp[loop];
            }

            // Reset current object
            m_moveDistancePixel = Vector2.Zero;
            m_wheelDelta = 0;
            for(int loop=0; loop<BUTTON_COUNT; loop++)
            {
                m_buttonsUp[loop] = false;
                
                if(m_buttonsHit[loop] || m_buttonsDown[loop])
                {
                    m_buttonsHit[loop] = false;
                    m_buttonsDown[loop] = true;
                }
                else
                {
                    m_buttonsHit[loop] = false;
                    m_buttonsDown[loop] = false;
                }
            }

            return result;
        }

        public Vector2 MoveDistanceDip
        {
            get { return m_moveDistancePixel; }
        }

        public Vector2 PositionDip
        {
            get { return m_positionPixel; }
        }

        public Vector2 MoveDistanceRelative
        {
            get
            {
                if (m_screenSizePixel.X == 0f) { return Vector2.Zero; }
                if (m_screenSizePixel.Y == 0f) { return Vector2.Zero; }

                return m_moveDistancePixel / m_screenSizePixel;
            }
        }

        public Vector2 PositionRelative
        {
            get
            {
                if(m_screenSizePixel.X == 0f) { return Vector2.Zero; }
                if(m_screenSizePixel.Y == 0f) { return Vector2.Zero; }

                return m_positionPixel / m_screenSizePixel;
            }
        }

        public int WheelDelta
        {
            get { return m_wheelDelta; }
        }
    }
}