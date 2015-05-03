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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Objects;

namespace SeeingSharp.Multimedia.Core
{
    public class ChangeOpacityToAnimation : AnimationBase
    {
        private float m_startOpacity;
        private TimeSpan m_duration;
        private float m_moveOpacity;
        private float m_targetOpacity;
        private GenericObject m_targetObject;

        /// <summary>
        /// Initialize a new Instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetOpacity">The target opacity.</param>
        /// <param name="duration">The duration.</param>
        /// <exception cref="System.Exception">Opacity value can be between 0 and 1, not greater than 1 and not lower than 0!</exception>
        public ChangeOpacityToAnimation(GenericObject targetObject, float targetOpacity, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            m_targetObject = targetObject;
            m_duration = duration;
            m_targetOpacity = targetOpacity;

            if (targetOpacity < 0f || targetOpacity > 1f)
            {
                throw new Exception("Opacity value can be between 0 and 1, not greater than 1 and not lower than 0!");
            }
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_startOpacity = m_targetObject.Opacity;
            m_moveOpacity = m_targetOpacity - m_startOpacity;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(UpdateState updateState, AnimationState animationState)
        {
            float changeFactor = (float)base.CurrentTime.Ticks / (float)base.FixedTime.Ticks;
            m_targetObject.Opacity = m_startOpacity + m_moveOpacity * changeFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.Opacity = m_targetOpacity;

            m_moveOpacity = 0;
            m_startOpacity = 1;
        }
    }
}