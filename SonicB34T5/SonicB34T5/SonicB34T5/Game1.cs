using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using form = System.Windows.Forms;

namespace SonicB34T5
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model mdl;
        NxContentLoader mLoader;
        NxEntity mEnt;
        NxCamera cam;
        NxInput nInput;
        SpriteFont myFont;
        NxDebugDraw nDebug;
        Effect ef;
        Effect basiceff; BasicEffect ba;

        Texture2D tex;
        public Game1()
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
            NxDebugShapeRenderer.Initialize(GraphicsDevice);
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
            //MessageBox.Show(Environment.CurrentDirectory);
            mLoader = new NxContentLoader(this);//Content.Load<Model>("cent");
            mdl =   mLoader.LoadModel(Environment.CurrentDirectory + "\\obo.fbx");
            cam = new NxCamera(GraphicsDevice.Viewport, new Vector3(0, 0, -500), Vector3.Zero, NxCamera.CameraType.Targeted);
            
            nInput = new NxInput(GraphicsDevice.Viewport);
            myFont = Content.Load<SpriteFont>("SpriteFont1");
            nDebug = new NxDebugDraw(GraphicsDevice);
            ef = Content.Load<Effect>("PixelMotionBlurNoMRT");
            basiceff = Content.Load<Effect>("Effect1");
            tex = Content.Load<Texture2D>("10220");
            ba = new BasicEffect(GraphicsDevice);
            mEnt = new NxEntity(mdl, cam,Content,GraphicsDevice,tex);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit

            if (this.IsActive)
            {
                if (nInput.IsKeyDown(Keys.Escape))
                    this.Exit();


                cam.mType = NxCamera.CameraType.Free;


                if (nInput.IsKeyDown(Keys.W))
                    cam.mPos.Z += gameTime.ElapsedGameTime.Milliseconds * .1f;
                if (nInput.IsKeyDown(Keys.S))
                    cam.mPos.Z -= gameTime.ElapsedGameTime.Milliseconds * .1f;
                if (nInput.IsKeyDown(Keys.A))
                    cam.mPos.X += gameTime.ElapsedGameTime.Milliseconds * .1f;
                if (nInput.IsKeyDown(Keys.D))
                    cam.mPos.X -= gameTime.ElapsedGameTime.Milliseconds * .1f;



                if (nInput.IsKeyDown(Keys.Q))
                    cam.mPos.Y += gameTime.ElapsedGameTime.Milliseconds * .1f;
                if (nInput.IsKeyDown(Keys.E))
                    cam.mPos.Y -= gameTime.ElapsedGameTime.Milliseconds * .1f;

                cam.mAngle.pan += nInput.mMouseForce.X * .2f;
                cam.mAngle.tilt += nInput.mMouseForce.Y * .2f;

                cam.Update();

                nInput.UpdateMouse();

            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            mEnt.UpdateOBB();
            
            mEnt.mAngle.pan += 1;
            mEnt.mAngle.tilt += 1;
          /*    Matrix mWorld = Matrix.CreateRotationX(MathHelper.ToRadians(mEnt.mAngle.tilt))
                       * Matrix.CreateRotationY(MathHelper.ToRadians(mEnt.mAngle.pan))
                       * Matrix.CreateRotationZ(MathHelper.ToRadians(mEnt.mAngle.roll))
                       * Matrix.CreateTranslation(mEnt.mPos);
            Matrix[] transforms = new Matrix[mEnt.mModel.Bones.Count];
            mEnt.mModel.CopyAbsoluteBoneTransformsTo(transforms);
              
            foreach (ModelMesh mesh in mEnt.mModel.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in(mesh.Effects))  
                { 
                   // effect.
                    effect.EnableDefaultLighting();

                    effect.World = transforms[mesh.ParentBone.Index] *  mWorld;
                   // mWorldMat = effect.World;
                    effect.View = cam.mView;
                    effect.Projection = cam.mProjection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
              
            
             basiceff.Parameters["WorldViewProj"].SetValue(mWorld* cam.mView*cam.mProjection);
             basiceff.Parameters["UserTexture"].SetValue(tex);
            
           // ef.Parameters["mWorld"].SetValue(mWorld);
          // ef.Parameters["mWorldViewProjection"].SetValue(mWorld* cam.mView*cam.mProjection);

             ;
            foreach (ModelMesh m in mEnt.mModel.Meshes)
            {
                foreach (ModelMeshPart mp in m.MeshParts )
                {
                    mp.Effect = basiceff;
                    
                }
                m.Draw();
            }


          
            mEnt.Draw();
            nDebug.Begin(cam.mView, cam.mProjection);
            nDebug.DrawWireBox(mEnt.mOBB, Color.Black); 
            nDebug.DrawWireGrid(Vector3.UnitX * 6000, Vector3.UnitZ * 6000, new Vector3(-3000, 0, -3000), 100, 100, Color.Gray);
            nDebug.DrawRay(new Ray(Vector3.Zero, Vector3.Up * 3000), Color.Blue, 1);//X AXIS
            nDebug.DrawRay(new Ray(Vector3.Zero, Vector3.Right * 3000), Color.Green, 1);//Y AXIS
            nDebug.DrawRay(new Ray(Vector3.Zero, Vector3.Forward * 3000), Color.Yellow, 1);//Z AXIS
            //nDebug.DrawWireBox(mEnt.mBoundingBox, Color.Yellow);
            nDebug.End();

            // NxDebugShapeRenderer.AddBoundingSphere(mEnt.mModel.Meshes[0].BoundingSphere, Color.Red);
            // NxDebugShapeRenderer.AddBoundingBox(b, Color.GreenYellow);
            // NxDebugShapeRenderer.Draw(gameTime, cam.mView, cam.mProjection);
            */

            
            mEnt.Draw(spriteBatch,GraphicsDevice);
           
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);
            spriteBatch.DrawString(myFont, GraphicsDevice.GraphicsDeviceStatus.ToString(), Vector2.Zero, Color.Red);
         //   spriteBatch.Draw((Texture2D)GraphicsDevice.Textures[0], new Rectangle(0, 0, 100, 100), Color.White);
            spriteBatch.End();
          //  GraphicsDevice.DepthStencilState = DepthStencilState.Default;
           // GraphicsDevice.BlendState = BlendState.Opaque;
           // GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
          //  GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
