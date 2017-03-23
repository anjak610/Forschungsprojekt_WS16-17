using Fusee.Base.Common;
using System;

/// <summary>
/// Just a wrapper class for computing RGB values from HSV input
/// </summary>

namespace Fusee.Tutorial.Core
{
    public class Color
    {
        /*
        vec3 hsv2rgb(in vec3 c)
        {
            vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
            vec3 result = c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);

            return result;
        }
        */

        // r,g,b values are from 0 to 1
        // h = [0,360], s = [0,1], v = [0,1]
        // if s == 0, then h = -1 (undefined)
        
        public static void HSVtoRGB(out float r, out float g, out float b, float h, float s, float v)
        {
            int i;
            float f, p, q, t;
            if (s == 0)
            {
                // achromatic (grey)
                r = g = b = v;
                return;
            }
            h /= 60;            // sector 0 to 5
            i = (int) System.Math.Floor(h);
            f = h - i;          // factorial part of h
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));
            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:        // case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }
        }
    }
}
