using System.Runtime.InteropServices;

namespace UnityEngine.Rendering.Universal.Editor.Utility
{
    /// <summary>
    /// 模拟键盘输入
    /// 
    /// Author : Aorition
    /// 
    /// Update : 2023-03-17
    /// 
    /// </summary>
    public static class SimulateKeyboardInput
    {

        /// <summary>
        /// 模拟键盘输入
        /// </summary>
        /// <param name="bvk">虚拟键值 ESC键对应的是27 RightArrow键对应的是39</param>
        /// <param name="bScan">//0</param>
        /// <param name="dwFlags">//0为按下，1按住，2释放</param>
        /// <param name="dwExtraInfo">/0</param>
        [DllImport("user32.dll", EntryPoint = "keybd_event")]

        private static extern void DispatchKeyEvtInternal(byte bvk, byte bScan, int dwFlags, int dwExtraInfo);

        /// <summary>
        /// 发布键盘输入
        /// </summary>
        /// <param name="keyCode">虚拟键值</param>
        public static void Dispatch(SimulateKeyboardKeyCode keyCode)
        {
            DispatchKeyEvtInternal((byte)keyCode, 0, 0, 0);
        }

        /// <summary>
        /// 发布键盘输入
        /// </summary>
        /// <param name="keyCode">虚拟键值</param>
        /// <param name="action">虚拟按键行为</param>
        public static void Dispatch(SimulateKeyboardKeyCode keyCode, SimulateKeyboardAction action)
        {
            DispatchKeyEvtInternal((byte)keyCode, 0, (int)action, 0);
        }

    }

    /// <summary>
    /// 虚拟按键行为
    /// </summary>
    public enum SimulateKeyboardAction
    {
        Down = 0, //按下
        Hold = 1, //按住
        Release = 2 //释放
    }

    /// <summary>
    /// 虚拟键值
    /// </summary>
    public enum SimulateKeyboardKeyCode : byte
    {
        //其它键
        Backspace = 8,
        Tab = 9,
        Clear = 12,
        Enter = 13,
        Shift = 16,
        Ctl = 17,
        Alt = 18,
        CapsLock = 20,
        Esc = 27,
        Spacebar = 32,
        PageUp = 33,
        PageDown = 34,
        End = 35,
        Home = 36,
        LeftArrow = 37,
        UpArrow = 38,
        RightArrow = 39,
        DownArrow = 40,
        Insert = 45,
        Delete = 46,
        Help = 47,
        //字母和数字键
        Num0 = 48,
        Num1 = 49,
        Num2 = 50,
        Num3 = 51,
        Num4 = 52,
        Num5 = 53,
        Num6 = 54,
        Num7 = 55,
        Num8 = 56,
        Num9 = 57,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        //数字小键盘的键
        PadNum0 = 96,
        PadNum1 = 97,
        PadNum2 = 98,
        PadNum3 = 99,
        PadNum4 = 100,
        PadNum5 = 101,
        PadNum6 = 102,
        PadNum7 = 103,
        PadNum8 = 104,
        PadNum9 = 105,
        PadMultiply = 106, // *
        PadPlus = 107,    // +
        PadEnter = 108, // Enter
        PadMinus = 109, // -
        PadPeriod = 110, // .
        PadDivide = 111, // /
        //功能键
        F1 = 112,
        F2 = 113,
        F3 = 114,
        F4 = 115,
        F5 = 116,
        F6 = 117,
        F7 = 118,
        F8 = 119,
        F9 = 120,
        F10 = 121,
        F11 = 122,
        F12 = 123,
        //其它键
        NumLock = 144,

    }

}
