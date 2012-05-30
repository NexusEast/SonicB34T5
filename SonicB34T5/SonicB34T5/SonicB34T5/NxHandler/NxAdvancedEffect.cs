using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace NxHandler
{
    class NxAdvancedEffect
    {

        public Effect MotionBlurEffect; 
        public NxAdvancedEffect(ContentManager c)
        {
            MotionBlurEffect = c.Load<Effect>("PixelMotionBlurNoMRT");
        }
    }
}
