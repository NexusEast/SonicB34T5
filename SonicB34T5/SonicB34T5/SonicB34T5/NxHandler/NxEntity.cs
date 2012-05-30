using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SonicB34T5
{
    struct NxAngle
    {

        public float pan;
        public float tilt;
        public float roll;
    }
    class NxEntity
    {
        public Model mModel;
        public Vector3 mPos;
        public Vector3 mScale;
        public NxAngle mAngle;
        private Matrix mWorld;
        public Matrix mWorldMat;
        public NxCamera mCam;
        public NxOBB mOBB;
        public BoundingBox mBoundingBox;
        static bool mDOFEnabled = true;
        public BasicEffect mBasicEffect = null;
        //static 

        private Rectangle DrawQuad;
        public Texture2D mTexture;
        #region MotionBlurFeild
        protected static Effect mMotionBlurEffect = null;
        static bool mMotionBlurEnabled = true;
        private Matrix
            mWorldViewProj,
            mWorldViewProjLast;
        private RenderTarget2D
            mRTColor,
            mRTVelocity,
            mRTVelocityLast;
        #endregion


        public string debugMsg;

        private void InitMotionBlurRTs(GraphicsDevice g)
        {
            mRTColor = new RenderTarget2D(g, g.Viewport.Width, g.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            mRTVelocity =
                new RenderTarget2D(g, g.Viewport.Width, g.Viewport.Height, false, SurfaceFormat.HalfVector2, DepthFormat.None);
            mRTVelocityLast =
                new RenderTarget2D(g, g.Viewport.Width, g.Viewport.Height, false, SurfaceFormat.HalfVector2, DepthFormat.None);
       
        }







        public NxEntity(Model m, NxCamera cam,
            ContentManager c, GraphicsDevice g,
            Texture2D Texture)
        {
            mTexture = Texture;
            InitMotionBlurRTs(g);
            DrawQuad = new Rectangle(0,0,mRTColor.Width,mRTColor.Height);
            if (mBasicEffect == null)
                mBasicEffect = new BasicEffect(g);
            if (mMotionBlurEffect == null)
                mMotionBlurEffect = c.Load<Effect>("PixelMotionBlurNoMRT");
            mScale = Vector3.One;
            mCam = cam;
            mModel = m;
            mWorld = Matrix.CreateRotationX(MathHelper.ToRadians(mAngle.tilt))
                        * Matrix.CreateRotationY(MathHelper.ToRadians(mAngle.pan))
                        * Matrix.CreateRotationZ(MathHelper.ToRadians(mAngle.roll))
                        * Matrix.CreateTranslation(mPos);
            Matrix[] transforms = new Matrix[mModel.Bones.Count];
            mModel.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in mModel.Meshes)
            {

                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * mWorld;
                    mWorldMat = effect.World;
                }

            }

            mBoundingBox = UpdateBoundingBox();
            mOBB = UpdateOBB();

            Viewport viewport = g.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            mMotionBlurEffect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection); 
        }

        public NxOBB UpdateOBB()
        {
            Quaternion a = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(mAngle.pan));
            a *= Quaternion.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(mAngle.tilt));
            a *= Quaternion.CreateFromAxisAngle(Vector3.Forward, MathHelper.ToRadians(mAngle.roll));

            return mOBB = new NxOBB(mPos, mBoundingBox.Max, a);
        }



        public BoundingBox UpdateBoundingBox()
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);



            // For each mesh of the model
            foreach (ModelMesh mesh in mModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]),
                            mWorldMat);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            // Create and return bounding box
            return mBoundingBox = new BoundingBox(min, max);
        }

        public void Draw(SpriteBatch s, GraphicsDevice g)
        {
            g.SamplerStates[0] = SamplerState.PointClamp;
            g.SamplerStates[1] = SamplerState.PointClamp;
            // Copy any parent transforms.
            mWorld = Matrix.CreateRotationX(MathHelper.ToRadians(mAngle.tilt))
                        * Matrix.CreateRotationY(MathHelper.ToRadians(mAngle.pan))
                        * Matrix.CreateRotationZ(MathHelper.ToRadians(mAngle.roll))
                        * Matrix.CreateTranslation(mPos);
            Matrix[] transforms = new Matrix[mModel.Bones.Count];
            mModel.CopyAbsoluteBoneTransformsTo(transforms);
            /*
            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in mModel.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    
                    effect.World = transforms[mesh.ParentBone.Index] * mWorld;
                    mWorldMat = effect.World;
                    effect.View = mCam.mView;
                    effect.Projection = mCam.mProjection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            */

            if (mMotionBlurEnabled)
            {

                mBasicEffect.View = mCam.mView;
                mBasicEffect.World = mWorldMat;
                mBasicEffect.Projection = mCam.mProjection;

                g.SetRenderTarget(mRTColor);
                g.Clear(Color.Blue);
                foreach (ModelMesh m in mModel.Meshes)
                {
                    foreach (ModelMeshPart mp in m.MeshParts)
                    {
                        mBasicEffect.TextureEnabled = true;
                        mBasicEffect.Texture = mTexture;
                        mBasicEffect.World = transforms[m.ParentBone.Index] * mWorld;
                        mWorldMat = mBasicEffect.World;
                        mBasicEffect.LightingEnabled = true;

                        mp.Effect = mBasicEffect;

                    }
                    m.Draw();
                }
            }

            g.SetRenderTarget(null);
            mWorldViewProjLast = mWorldViewProj;
            mWorldViewProj = mWorldMat * mCam.mView * mCam.mProjection;
            mRTVelocityLast = mRTVelocity;
            g.SetRenderTarget(mRTVelocity);
            g.Clear(Color.Blue);
                    mMotionBlurEffect.Parameters["mWorld"].SetValue(mWorldMat);
                    mMotionBlurEffect.Parameters["mWorldViewProjection"].SetValue(mWorldViewProj);
                    mMotionBlurEffect.Parameters["mWorldViewProjectionLast"].SetValue(mWorldViewProjLast);
                    mMotionBlurEffect.CurrentTechnique = mMotionBlurEffect.Techniques["WorldWithVelocity"];
            foreach (ModelMesh m in mModel.Meshes)
            {
                foreach (ModelMeshPart mp in m.MeshParts)
                {
                    mp.Effect = mMotionBlurEffect;

                }
                m.Draw();
            }
            g.SetRenderTarget(null);

            mMotionBlurEffect.Parameters["CurFrameVelocityTexture"].SetValue(mRTVelocity);
            mMotionBlurEffect.Parameters["LastFrameVelocityTexture"].SetValue(mRTVelocityLast);
            mMotionBlurEffect.Parameters["RenderTargetTexture"].SetValue(mRTColor);
           
            mMotionBlurEffect.CurrentTechnique = mMotionBlurEffect.Techniques["PostProcessMotionBlur_2_0"];
            s.Begin(SpriteSortMode.Texture, BlendState.Opaque, SamplerState.AnisotropicClamp ,
                DepthStencilState.Default, RasterizerState.CullCounterClockwise, mMotionBlurEffect);
            s.Draw((Texture2D)mRTColor, DrawQuad, Color.White);
            s.End();
            
            // s.Begin();
            // s.Draw(mRTColor, new Rectangle(0, 0, 200, 120), Color.White);
            //s.End();


            /*
            Matrix[] transforms = new Matrix[mModel.Bones.Count];
            mModel.CopyAbsoluteBoneTransformsTo(transforms);
            mWorld = Matrix.CreateTranslation(mPos) ;
            mWorld *= Matrix.CreateScale(mScale);
            mWorld *= Matrix.CreateRotationX(MathHelper.ToRadians(mAngle.tilt));
            mWorld *= Matrix.CreateRotationY(MathHelper.ToRadians(mAngle.pan));
            mWorld *= Matrix.CreateRotationZ(MathHelper.ToRadians(mAngle.roll));
            foreach (ModelMesh m in mModel.Meshes)
            {
                foreach (BasicEffect be in m.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = mCam.mProjection;
                    be.View = mCam.mView;
                    be.World = mWorld * transforms[m.ParentBone.Index];
                }
                m.Draw();
            }


            */
        }
    }
}
