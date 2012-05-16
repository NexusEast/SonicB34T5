using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace SonicB34T5
{
    class NxContentLoader
    {
        static ContentBuilder contentBuilder;
        static ContentManager contentManager; 
        public NxContentLoader(Game1 g)
        {
            contentBuilder = new ContentBuilder();
            contentManager = new ContentManager(g.Services, contentBuilder.OutputDirectory);
        }


       public   Model LoadModel(string fileName)
        {
          

            // Unload any existing model. 
            contentManager.Unload();

            // Tell the ContentBuilder what to build.
            contentBuilder.Clear();
            contentBuilder.Add(fileName, "Model", null, "ModelProcessor");

            // Build this new model data.
            string buildError = contentBuilder.Build();

            if (string.IsNullOrEmpty(buildError))
            {
                // If the build succeeded, use the ContentManager to
                // load the temporary .xnb file that we just created.
                return contentManager.Load<Model>("Model");
            }
            else
               
            { // If the build failed, display an error message.
                MessageBox.Show(buildError, "Error");
                return null;
            }

            
        }
    }
}
