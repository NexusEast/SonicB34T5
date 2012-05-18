using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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
       public   Model mModel;
       public Vector3 mPos;
       public Vector3 mScale;
       public NxAngle mAngle;
       private  Matrix mWorld;
       public Matrix mWorldMat;
       public NxCamera mCam;
       public NxOBB mOBB;
       public BoundingBox mBoundingBox; 
        public NxEntity(Model m,NxCamera cam)
       {
        
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
        }

        public NxOBB UpdateOBB()
        {
            Quaternion a = Quaternion.CreateFromAxisAngle(Vector3.Up,MathHelper.ToRadians( mAngle.pan));
            a *= Quaternion.CreateFromAxisAngle(Vector3.Right,MathHelper.ToRadians( mAngle.tilt));
            a *= Quaternion.CreateFromAxisAngle(Vector3.Forward,MathHelper.ToRadians( mAngle.roll));

            return  mOBB = new NxOBB(mPos,mBoundingBox.Max,a);
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

        public void Draw()
        {
          //  mDebugVec3 = Matrix.

            // Copy any parent transforms.
            mWorld = Matrix.CreateRotationX(MathHelper.ToRadians(mAngle.tilt))
                        * Matrix.CreateRotationY(MathHelper.ToRadians(mAngle.pan))
                        * Matrix.CreateRotationZ(MathHelper.ToRadians(mAngle.roll))
                        * Matrix.CreateTranslation(mPos);
            Matrix[] transforms = new Matrix[mModel.Bones.Count];
            mModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in mModel.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index]*mWorld;
                    mWorldMat = effect.World;
                    effect.View = mCam.mView;
                    effect.Projection = mCam.mProjection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            
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
