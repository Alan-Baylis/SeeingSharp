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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    public class CallActionAnimation : AnimationBase
    {
        private Action m_actionToCall;
        private Action m_cancelAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallActionAnimation" /> class.
        /// </summary>
        public CallActionAnimation(Action actionToCall)
            : base(null, AnimationType.FixedTime, TimeSpan.Zero)
        {
            m_actionToCall = actionToCall;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallActionAnimation"/> class.
        /// </summary>
        /// <param name="actionToCall">The action to call.</param>
        /// <param name="cancelAction">The cancel action.</param>
        public CallActionAnimation(Action actionToCall, Action cancelAction)
            : base(null, AnimationType.FixedTime, TimeSpan.Zero)
        {
            m_actionToCall = actionToCall;
            m_cancelAction = cancelAction;
        }

        /// <summary>
        /// Called when this animation was canceled.
        /// </summary>
        public override void OnCanceled()
        {
            if (m_cancelAction != null)
            {
                m_cancelAction();
            }
        }

        /// <summary>
        /// Called when this animation was finished.
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            if(m_actionToCall != null)
            {
                m_actionToCall();
            }
        }
    }
}