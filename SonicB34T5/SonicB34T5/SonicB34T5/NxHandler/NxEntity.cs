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
       public Matrix mWorld;
       public NxCamera mCam;
        public NxEntity(Model m,NxCamera cam)
        {
            mScale = Vector3.One;
            mCam = cam;
            mModel = m;
        }


        public void Draw()
        {
            mWorld = Matrix.CreateTranslation(mPos);
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
                    be.World = mWorld;
                }
                m.Draw();
            }
        }
    }
}
