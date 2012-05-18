using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SonicB34T5
{
    
    class NxInput
    {
        private MouseState originalMouseState;
        public MouseState currentState;
        public Vector2 mMousePos;
        public Vector2 mMouseForce;
        int width;
        int height;
        public NxInput(Viewport v)
        {
            width = v.Width;
            height = v.Height;

            Mouse.SetPosition(v.Width / 2, v.Height / 2);
            originalMouseState = Mouse.GetState();
        }

       

        public bool IsKeyDown(Keys k)
        {
            return Keyboard.GetState().IsKeyDown(k);
        }

        public bool IsKeyUp(Keys k)
        {
            return Keyboard.GetState().IsKeyUp(k);
        }

        public void UpdateMouse()
        {
            
             currentState = Mouse.GetState();
            if (currentState != originalMouseState)
            {
                mMouseForce.X = currentState.X - originalMouseState.X;
                mMouseForce.Y = currentState.Y - originalMouseState.Y;
                Mouse.SetPosition(width / 2, height / 2);

            }
            else mMouseForce = Vector2.Zero;
        }
    }
}
