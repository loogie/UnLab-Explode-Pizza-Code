using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGame
{
    class Sprite
    {
        public Vector2 position;
        public float rotation;
        public bool flip = false;

        public Texture2D texture { get; set; }
        public Rectangle[] frames { get; set; }
        
        public int cFrame = 0;

        public Vector2 origin { get; set; }

        public Sprite()
        {
 
        }

        private float oldTime = -1f;

        public void Update(GameTime gameTime)
        {
            if (oldTime == -1f)
            {
                oldTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            float newTime = (float)gameTime.TotalGameTime.TotalMilliseconds;

            if ( (newTime - oldTime) > ((float)1000 / 8f))
            {
                if (cFrame == frames.Length - 1)
                {
                    cFrame = 0;
                }
                else
                {
                    cFrame++;
                }

                oldTime = newTime;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            SpriteEffects effect;
            if (flip)
            {
                effect = SpriteEffects.FlipHorizontally;
            }
            else
            {
                effect = SpriteEffects.None;
            }

            sb.Draw(texture, position, frames[cFrame] , Color.White, rotation, origin, 1f, effect, 1f);           
        }
    }
}
