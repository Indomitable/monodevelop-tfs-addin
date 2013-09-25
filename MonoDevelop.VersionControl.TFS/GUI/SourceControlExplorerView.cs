using System;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class SourceControlExplorerView : AbstractXwtViewContent
    {
        #region implemented abstract members of AbstractViewContent

        public override void Load(string fileName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region implemented abstract members of AbstractXwtViewContent

        public override Widget Widget
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion


    }
}

