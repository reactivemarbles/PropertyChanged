// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// The type of receiver.
    /// </summary>
    public enum ReceiverKind
    {
        /// <summary>
        /// The receiver is a 'this' reference.
        /// </summary>
        This,

        /// <summary>
        /// The receiver is a instance reference.
        /// </summary>
        Instance,
    }
}
