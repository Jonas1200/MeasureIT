// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HoloToolkit.Examples.GazeRuler
{
    /// <summary>
    /// any geometry class inherit this interface should be closeable
    /// </summary>
    public interface IRectangleClosable
    {
        //finish special polygon
        void CloseRectangle(GameObject LinePrefab, GameObject TextPrefab);
    }
}