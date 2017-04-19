﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Math.Core;

/// <summary>
/// Represents a simple point which holds all properties given by the IPM format.
/// </summary>

namespace Fusee.Forschungsprojekt.Core
{
    public class Point
    {
        public float3 Position;
        public float3 Color;
        public float EchoId;
        public float ScanNr;
    }
}