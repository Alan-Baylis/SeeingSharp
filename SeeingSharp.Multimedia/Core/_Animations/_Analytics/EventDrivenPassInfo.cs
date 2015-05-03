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

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// Holds some detail information about event-driven animation calculation.
    /// </summary>
    public class EventDrivenPassInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDrivenPassInfo"/> class.
        /// </summary>
        /// <param name="steps">All steps performed in this calculation.</param>
        internal EventDrivenPassInfo(List<EventDrivenStepInfo> steps)
        {
            this.CountSteps = steps.Count;
            this.Steps = steps;
        }

        public List<EventDrivenStepInfo> Steps
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total time the animation took (simulation time).
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                if (Steps == null) { return TimeSpan.Zero; }

                TimeSpan totalTime = TimeSpan.Zero;
                foreach(var actAnimStep in this.Steps)
                {
                    totalTime = totalTime + actAnimStep.UpdateTime;
                }

                return totalTime;
            }
        }

        /// <summary>
        /// Total count of calculation steps.
        /// </summary>
        public int CountSteps
        {
            get;
            private set;
        }
    }
}