// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-11-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-11-2026
// ***********************************************************************
// <copyright file="ServiceEmblemExtensions.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace ServePoint.Cadet.Extensions;

/// <summary>
/// Class ServiceEmblemExtensions.
/// </summary>
public static class ServiceEmblemExtensions
{
    extension(ServiceEmblem serviceEmblem)
    {
        /// <summary>
        /// Converts to imagepath.
        /// </summary>
        /// <returns>string?.</returns>
        public string? ToImagePath()
        {
            const string directory = "images";

            return serviceEmblem == ServiceEmblem.None ? null : $"{directory}/{serviceEmblem}.png";
        }

        /// <summary>
        /// Converts to display.
        /// </summary>
        /// <returns>string.</returns>
        public string ToDisplay()
        {
            switch (serviceEmblem)
            {
                case ServiceEmblem.Usa:
                    return "U.S. Army";
                case ServiceEmblem.Usn:
                    return "U.S. Navy";
                case ServiceEmblem.Usmc:
                    return "U.S. Marine Corps";
                case ServiceEmblem.Usaf:
                    return "U.S. Air Force";
                case ServiceEmblem.Ussf:
                    return "U.S. Space Force";
                case ServiceEmblem.Uscg:
                    return "U.S. Coast Guard";
                case ServiceEmblem.None:
                default:
                    return "None";
            }
        }
    }
}