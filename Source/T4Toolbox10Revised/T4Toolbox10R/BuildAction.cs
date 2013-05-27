﻿// <copyright file="BuildAction.cs" company="T4 Toolbox Team">
//  Copyright © T4 Toolbox Team. All Rights Reserved.
// </copyright>

namespace T4Toolbox
{
    /// <summary>
    /// Defines the known Build Actions for Visual Studio project items.
    /// </summary>
    public static class BuildAction
    {
        /// <summary>
        /// No action is taken.
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// The file is compiled.
        /// </summary>
        public const string Compile = "Compile";

        /// <summary>
        /// The file is included in the Content project output group.
        /// </summary>
        public const string Content = "Content";

        /// <summary>
        /// The file is included in the main generated assembly or in a satellite assembly as a resource.
        /// </summary>
        public const string EmbeddedResource = "EmbeddedResource";
    }
}
