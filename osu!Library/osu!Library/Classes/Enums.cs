using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_Library.Classes
{
    #region Enums
    public enum MenuSection
    {
        None,
        Settings,
        About
    }

    public enum AnimationProgressStage
    {
        Active,
        Finished
    }

    public enum AnimationAction
    {
        Open,
        Close
    }

    public enum WindowMode
    {
        Window,
        Overlay
    }

    public enum PlayMode
    {
        Play,
        Pause,
        Stop,
        Unloaded
    }
    #endregion
}
