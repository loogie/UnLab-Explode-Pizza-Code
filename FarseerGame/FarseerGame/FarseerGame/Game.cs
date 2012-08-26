using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FarseerGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        DebugViewXNA debugView;

        World world;

        Matrix proj;
        Matrix view;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            world = new World(new Vector2(0f, 20f));
            Utils.initScale(25f);

            debugView = new DebugViewXNA(world);
            debugView.LoadContent(GraphicsDevice, Content);
            debugView.DefaultShapeColor = Color.White;
            debugView.InactiveShapeColor = Color.Gray;

            proj = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            view = Matrix.CreateScale(Utils.scale);

            Body ground = BodyFactory.CreateRectangle(world, Utils.toPhysicsScale(GraphicsDevice.Viewport.Width), Utils.toPhysicsScale(40f), 1f);
            ground.Position = new Vector2(Utils.toPhysicsScale(GraphicsDevice.Viewport.Width / 2), Utils.toPhysicsScale(GraphicsDevice.Viewport.Height));

            Fixture side = FixtureFactory.AttachRectangle(Utils.toPhysicsScale(20f), Utils.toPhysicsScale(100f), 1f, new Vector2(Utils.toPhysicsScale(-(GraphicsDevice.Viewport.Width / 2)), Utils.toPhysicsScale(-100f)), ground);

            Vertices v = new Vertices();
            v.Add(new Vector2(Utils.toPhysicsScale(400f), Utils.toPhysicsScale(200f)));
            v.Add(new Vector2(Utils.toPhysicsScale(600f), Utils.toPhysicsScale(100f)));
            v.Add(new Vector2(Utils.toPhysicsScale(600f), Utils.toPhysicsScale(200f)));

            g2 = BodyFactory.CreatePolygon(world, v, 1f);
            g2.BodyType = BodyType.Dynamic;
            g2.Mass = 1000f;
            g2.Friction = float.MaxValue;
            g2.Position = new Vector2(0f, 0f);

            rect = BodyFactory.CreateRectangle(world, Utils.toPhysicsScale(43.5f), Utils.toPhysicsScale(64f), 1f);
            rect.Position = new Vector2(0f, Utils.toPhysicsScale(-25f));
            rect.BodyType = BodyType.Dynamic;
            rect.Restitution = 0f;
            rect.Friction = 0f;

            circle = BodyFactory.CreateCircle(world, Utils.toPhysicsScale(20f), 1f);
            circle.BodyType = BodyType.Dynamic;
            circle.Restitution = 0f;
            circle.Friction = 0.5f;
            circle.OnCollision += new OnCollisionEventHandler(circle_OnCollision);
            circle.OnSeparation += new OnSeparationEventHandler(circle_OnSeparation);
            circle.IsBullet = true;

            rect.IgnoreCollisionWith(circle);
            circle.IgnoreCollisionWith(rect);

            JointFactory.CreateFixedAngleJoint(world, rect);
            motor = JointFactory.CreateRevoluteJoint(world, rect, circle, new Vector2(0f, 0f));
            motor.MotorEnabled = true;
            motor.MaxMotorTorque = 1000f;
            motor.MotorSpeed = 0f;
            motor.LimitEnabled = false;

            circle.Friction = float.MaxValue;

            Texture2D atlas = Content.Load<Texture2D>("atlas");

            List<Rectangle> frames = new List<Rectangle>();
            
            frames.Add(new Rectangle(0, 0, 44, 75));
            frames.Add(new Rectangle(0, 75, 44, 75));
            frames.Add(new Rectangle(0, 151, 44, 75));
            frames.Add(new Rectangle(44, 0, 44, 75));
            frames.Add(new Rectangle(87, 0, 44, 75));
            frames.Add(new Rectangle(131, 0, 44, 75));
            frames.Add(new Rectangle(174, 0, 44, 75));
            frames.Add(new Rectangle(44, 75, 44, 75));
            frames.Add(new Rectangle(44, 151, 44, 75));
            frames.Add(new Rectangle(87, 75, 44, 75));
            frames.Add(new Rectangle(87, 151, 44, 75));
            frames.Add(new Rectangle(131, 75, 44, 75)); 

            guy = new Sprite();
            guy.texture = atlas;
            guy.frames = frames.ToArray<Rectangle>();
            guy.origin = new Vector2(21.75f, 32f);

        }

        Sprite guy;

        bool g2_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            g2.CollisionCategories = Category.Cat2;
            g2.BodyType = BodyType.Static;

            return false;
        }

        void circle_OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            canJump = false;
        }

        bool circle_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            canJump = true;

            return true;
        }

        bool canJump = false;
        RevoluteJoint motor;
        Body rect;
        Body circle;
        Body g2;

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        bool lMouseDown = false;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();

            if (ms.LeftButton == ButtonState.Pressed)
            {
                lMouseDown = true;
            }
            else
            {
                if (lMouseDown)
                {
                    Random rand = new Random((int)DateTime.Now.Ticks);

                    for (int i = 0; i < 5; i++)
                    {
                        Body box = BodyFactory.CreateRectangle(world, 2f, 2f, 1f);
                        box.Position = new Vector2(Utils.toPhysicsScale(ms.X) + (rand.Next(1, 10) - 5), Utils.toPhysicsScale(ms.Y));
                        box.BodyType = BodyType.Dynamic;
                    }

                    lMouseDown = false;
                }
            }

            //Input
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.A))
            {
                guy.flip = true;
                motor.MotorSpeed = -20f;
                
                if (!canJump)
                {
                    circle.ApplyForce(new Vector2(-40f,0f));
                }
            }
            else if (ks.IsKeyDown(Keys.D))
            {
                guy.flip = false;
                motor.MotorSpeed = 20f;

                if (!canJump)
                {
                    circle.ApplyForce(new Vector2(40f,0f));
                }
            }
            else
            {
                motor.MotorSpeed = 0f;
            }

            if (ks.IsKeyDown(Keys.W))
            {
                if (canJump)
                {
                    Vector2 jump = new Vector2(0f, -25f);
                    rect.ApplyLinearImpulse(ref jump);
                }
            }

            Vector2 pos = new Vector2(Utils.toDisplayScale(rect.Position.X), Utils.toDisplayScale(rect.Position.Y));
            guy.position = pos;

            guy.Update(gameTime);

            world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            guy.Draw(spriteBatch);
            spriteBatch.End();

            debugView.RenderDebugData(ref proj, ref view);

            base.Draw(gameTime);
        }
    }
}
