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
        float pan;
        float tilt;
        float roll;
    }
    class NxEntity
    {
       public   Model mModel;
       public Vector3 mPos;
       public Vector3 mScale;
       public Matrix mWorld;
       public Matrix mProjectino;
       public Matrix mView;
       public NxCamera mCam;
        public NxEntity(Model m,NxCamera cam)
        {
            mCam = cam;
            mModel = m;
        }


        public void Draw()
        {
            foreach (ModelMesh m in mModel.Meshes)
            {
                foreach (BasicEffect be in m.Effects)
                {
                    be.Projection = mCam.mProjection;
                    be.View = mCam.mView;
                }
            }
        }
    }
}
