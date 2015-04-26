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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureNotEmpty(
            this RectangleF rectangle, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((rectangle.Width <= 0f) ||
                (rectangle.Height <= 0f))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Rectangle {0} within method {1} must not be empty (given rectangle: {2})!",
                    checkedVariableName, callerMethod, rectangle));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotEmpty(
            this Rectangle rectangle, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((rectangle.Width <= 0f) ||
                (rectangle.Height <= 0f))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Rectangle {0} within method {1} must not be empty (given rectangle: {2})!",
                    checkedVariableName, callerMethod, rectangle));
            }
        }
    }
}
