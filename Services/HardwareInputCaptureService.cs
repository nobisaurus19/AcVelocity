using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AcVelocity.Services
{
    public class HardwareInputSubmittedResult
    {
        public bool IsCancelled { get; set; }
        public bool IsWheelButton { get; set; }
        public int JoyIndex { get; set; } = -1;
        public int ButtonIndex { get; set; } = -1;
        public bool IsXboxButton { get; set; }
        public string XboxButton { get; set; } = string.Empty;
        public bool IsKeyboardKey { get; set; }
        public string KeyHex { get; set; } = string.Empty;
    }

    public class HardwareInputCaptureService
    {
        #region WinMM P/Invoke for DirectInput / Wheel Joysticks
        [StructLayout(LayoutKind.Sequential)]
        private struct JOYINFOEX
        {
            public int dwSize;
            public int dwFlags;
            public int dwXpos;
            public int dwYpos;
            public int dwZpos;
            public int dwRpos;
            public int dwUpos;
            public int dwVpos;
            public int dwButtons;
            public int dwButtonNumber;
            public int dwPOV;
            public int dwReserved1;
            public int dwReserved2;
        }

        private const int JOYERR_NOERROR = 0;
        private const int JOY_RETURNBUTTONS = 0x00000080;

        [DllImport("winmm.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);
        #endregion

        #region XInput P/Invoke for Xbox Gamepads
        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        private const int ERROR_SUCCESS = 0;

        private const ushort XINPUT_GAMEPAD_DPAD_UP = 0x0001;
        private const ushort XINPUT_GAMEPAD_DPAD_DOWN = 0x0002;
        private const ushort XINPUT_GAMEPAD_DPAD_LEFT = 0x0004;
        private const ushort XINPUT_GAMEPAD_DPAD_RIGHT = 0x0008;
        private const ushort XINPUT_GAMEPAD_START = 0x0010;
        private const ushort XINPUT_GAMEPAD_BACK = 0x0020;
        private const ushort XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
        private const ushort XINPUT_GAMEPAD_RIGHT_THUMB = 0x0080;
        private const ushort XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100;
        private const ushort XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;
        private const ushort XINPUT_GAMEPAD_A = 0x1000;
        private const ushort XINPUT_GAMEPAD_B = 0x2000;
        private const ushort XINPUT_GAMEPAD_X = 0x4000;
        private const ushort XINPUT_GAMEPAD_Y = 0x8000;

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        private static extern int XInputGetState1_4(int dwUserIndex, ref XINPUT_STATE pState);

        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState")]
        private static extern int XInputGetState1_3(int dwUserIndex, ref XINPUT_STATE pState);

        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")]
        private static extern int XInputGetState9_1_0(int dwUserIndex, ref XINPUT_STATE pState);

        private static int SafeXInputGetState(int userIndex, ref XINPUT_STATE state)
        {
            try
            {
                return XInputGetState1_4(userIndex, ref state);
            }
            catch (DllNotFoundException)
            {
                try
                {
                    return XInputGetState1_3(userIndex, ref state);
                }
                catch (DllNotFoundException)
                {
                    try
                    {
                        return XInputGetState9_1_0(userIndex, ref state);
                    }
                    catch
                    {
                        return -1;
                    }
                }
            }
            catch
            {
                return -1;
            }
        }
        #endregion

        public static string KeyToHex(Key key)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            return $"0x{vk:X2}";
        }

        public async Task<HardwareInputSubmittedResult> WaitForHardwareButtonAsync(CancellationToken ct)
        {
            return await Task.Run(async () =>
            {
                int structSize = Marshal.SizeOf(typeof(JOYINFOEX));

                // 1. Initial snapshot to ignore buttons that are already depressed when capture starts
                var initialJoyButtons = new int[8];
                for (int j = 0; j < 8; j++)
                {
                    var ji = new JOYINFOEX { dwSize = structSize, dwFlags = JOY_RETURNBUTTONS };
                    if (joyGetPosEx(j, ref ji) == JOYERR_NOERROR)
                    {
                        initialJoyButtons[j] = ji.dwButtons;
                    }
                }

                var initialPadButtons = new ushort[4];
                var initialPadTriggers = new bool[4, 2]; // 0=LT, 1=RT
                for (int p = 0; p < 4; p++)
                {
                    var state = new XINPUT_STATE();
                    if (SafeXInputGetState(p, ref state) == ERROR_SUCCESS)
                    {
                        initialPadButtons[p] = state.Gamepad.wButtons;
                        initialPadTriggers[p, 0] = state.Gamepad.bLeftTrigger > 120;
                        initialPadTriggers[p, 1] = state.Gamepad.bRightTrigger > 120;
                    }
                }

                while (!ct.IsCancellationRequested)
                {
                    // 2. Poll WinMM Joysticks (0..7)
                    for (int j = 0; j < 8; j++)
                    {
                        var ji = new JOYINFOEX { dwSize = structSize, dwFlags = JOY_RETURNBUTTONS };
                        if (joyGetPosEx(j, ref ji) == JOYERR_NOERROR)
                        {
                            int currentButtons = ji.dwButtons;
                            // Clear buttons from initial mask that have been released
                            initialJoyButtons[j] &= currentButtons;

                            for (int b = 0; b < 32; b++)
                            {
                                int mask = 1 << b;
                                if ((currentButtons & mask) != 0 && (initialJoyButtons[j] & mask) == 0)
                                {
                                    return new HardwareInputSubmittedResult
                                    {
                                        IsWheelButton = true,
                                        JoyIndex = j,
                                        ButtonIndex = b
                                    };
                                }
                            }
                        }
                    }

                    // 3. Poll XInput Gamepads (0..3)
                    for (int p = 0; p < 4; p++)
                    {
                        var state = new XINPUT_STATE();
                        if (SafeXInputGetState(p, ref state) == ERROR_SUCCESS)
                        {
                            ushort buttons = state.Gamepad.wButtons;
                            initialPadButtons[p] &= buttons;

                            string? pressedXButton = GetPressedXboxButton(buttons, initialPadButtons[p]);
                            if (pressedXButton != null)
                            {
                                return new HardwareInputSubmittedResult
                                {
                                    IsXboxButton = true,
                                    XboxButton = pressedXButton
                                };
                            }

                            // Check Triggers
                            bool ltPressed = state.Gamepad.bLeftTrigger > 120;
                            if (!ltPressed) initialPadTriggers[p, 0] = false;
                            else if (!initialPadTriggers[p, 0])
                            {
                                return new HardwareInputSubmittedResult
                                {
                                    IsXboxButton = true,
                                    XboxButton = "LT"
                                };
                            }

                            bool rtPressed = state.Gamepad.bRightTrigger > 120;
                            if (!rtPressed) initialPadTriggers[p, 1] = false;
                            else if (!initialPadTriggers[p, 1])
                            {
                                return new HardwareInputSubmittedResult
                                {
                                    IsXboxButton = true,
                                    XboxButton = "RT"
                                };
                            }
                        }
                    }

                    try
                    {
                        await Task.Delay(20, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                return new HardwareInputSubmittedResult { IsCancelled = true };
            }, ct);
        }

        private static string? GetPressedXboxButton(ushort current, ushort initial)
        {
            var candidates = new (ushort Mask, string Name)[]
            {
                (XINPUT_GAMEPAD_A, "A"),
                (XINPUT_GAMEPAD_B, "B"),
                (XINPUT_GAMEPAD_X, "X"),
                (XINPUT_GAMEPAD_Y, "Y"),
                (XINPUT_GAMEPAD_LEFT_SHOULDER, "LB"),
                (XINPUT_GAMEPAD_RIGHT_SHOULDER, "RB"),
                (XINPUT_GAMEPAD_START, "START"),
                (XINPUT_GAMEPAD_BACK, "BACK"),
                (XINPUT_GAMEPAD_LEFT_THUMB, "L_THUMB"),
                (XINPUT_GAMEPAD_RIGHT_THUMB, "R_THUMB"),
                (XINPUT_GAMEPAD_DPAD_UP, "DPAD_UP"),
                (XINPUT_GAMEPAD_DPAD_DOWN, "DPAD_DOWN"),
                (XINPUT_GAMEPAD_DPAD_LEFT, "DPAD_LEFT"),
                (XINPUT_GAMEPAD_DPAD_RIGHT, "DPAD_RIGHT"),
            };

            foreach (var (mask, name) in candidates)
            {
                if ((current & mask) != 0 && (initial & mask) == 0)
                {
                    return name;
                }
            }

            return null;
        }
    }
}
