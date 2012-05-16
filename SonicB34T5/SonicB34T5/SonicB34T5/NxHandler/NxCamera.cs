using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace SonicB34T5
{
    class NxCamera
    {


        protected enum CameraType
        {
            Targeted,
            Free,
        };


        public Matrix mView;
        public Matrix mProjection;
        public float mFov;
        public float mClipFar;
        public float mClipNear;
        public CameraType mType;
        public Vector3 mPos;
        public Vector3 mTgt;

  
        public NxCamera(Viewport v,Vector3 pos,Vector3 target,CameraType t)
        {
            mTgt = target;
            mPos = pos;
            mType = t;
            mType = CameraType.Targeted;
            mFov = 45;
            mClipNear = 1;
            mClipFar = 20000;
            mProjection = Matrix.CreatePerspectiveFieldOfView(mFov,
                v.AspectRatio, mClipNear, mClipFar);
            if (mType == CameraType.Targeted)
                mView = Matrix.CreateLookAt(mPos, mTgt, Vector3.Up);
            else
                mView = Matrix.CreateTranslation(pos);
            
        }

        public void Update()
        {

        }
    }
}
