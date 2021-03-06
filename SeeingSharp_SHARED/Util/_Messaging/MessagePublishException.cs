﻿#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace SeeingSharp.Util
{
    /// <summary>
    /// An exception info holding all information about exceptions occurred during publishing a message.
    /// </summary>
    public class MessagePublishException : SeeingSharpException
    {
        private Type m_messageType;
        private List<Exception> m_publishExceptions;
#if DESKTOP
        private string m_trueStackTrace;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePublishException"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="message">The message.</param>
        public MessagePublishException(Type messageType, string message)
            : base("Exceptions where raised while processing message of type " + messageType.FullName + ": " + message)
        {
            m_messageType = messageType;
            m_publishExceptions = new List<Exception>();

#if DESKTOP
            // Aquire true stacktrace information
            m_trueStackTrace = (new StackTrace()).ToString();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePublishException"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="publishExceptions">Exceptions raised during publish process.</param>
        public MessagePublishException(Type messageType, List<Exception> publishExceptions)
            : base("Exceptions where raised while processing message of type " + messageType.FullName + "!")
        {
            m_messageType = messageType;
            m_publishExceptions = publishExceptions;

            if (m_publishExceptions == null) { m_publishExceptions = new List<Exception>(); }

#if DESKTOP
            // Aquire true stacktrace information
            m_trueStackTrace = (new StackTrace()).ToString();
#endif
        }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        public Type MessageType
        {
            get { return m_messageType; }
        }

        /// <summary>
        /// Gets a list containing all exceptions.
        /// </summary>
        public List<Exception> PublishExceptions
        {
            get { return m_publishExceptions; }
        }

#if DESKTOP
        public string TrueStackTrace
        {
            get { return m_trueStackTrace; }
        }
#endif
    }
}
