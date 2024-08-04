using System;

namespace MageTest.Gui
{
    [Flags]
    public enum UIFlagsEnum
    {
        None = 0,
        NoThrobber = 1 << 0,
        HasFade = 1 << 1,
        FadeClickIsClose = 1 << 2,
        DontClose = 1 << 4,
        DisableAutoStash = 1 << 5
    }
    
    public enum UILayerEnum
    {
        None = 0,
        Main = 10,
        Windows = 20,
        Popups = 30,
        Loader = 40,
        Debug = 100
    }
}