using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarseerGame
{
    static class Utils
    {
        public static float scale;
        public static void initScale(float pixelsPerMeter)
        {
            scale = pixelsPerMeter;
        }

        public static float toPhysicsScale(float size)
        {
            return size / scale;
        }

        public static float toDisplayScale(float size)
        {
            return size * scale;
        }
    }
}
