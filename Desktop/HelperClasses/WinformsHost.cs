using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using static Fusee.Engine.Core.Input;
using Fusee.Engine.Imp.Graphics.Desktop;
using Fusee.Engine.Common;
using Fusee.Math.Core;
using MouseEventArgs = Fusee.Engine.Common.MouseEventArgs;
using KeyEventArgs = Fusee.Engine.Common.KeyEventArgs;
using System.Collections.Generic;
using System.Linq;

namespace Fusee.Tutorial.Desktop.HelperClasses
{
    /// <summary>
    ///     Instances of this class act as the glue between a Windows Forms renderControl and FUSEE when FUSEE is intended to display
    ///     its contents
    ///     on the Windows renderControl and to wire interactions to user input performed on the Windows renderControl.
    /// </summary>
    internal class WinformsHost : RenderCanvasWindowImp, IInputDriverImp
    {
        private bool _disposed;
        
        private Control _renderControl;
        private readonly Panel _parent;

        private int _mouseWheelPos;
        private bool _initialized;
        public RenderCanvasImp canvas = new RenderCanvasImp();

        private WinFormsKeyboardDeviceImp _keyboard;
        private WinFormsMouseDeviceImp _mouse;


        public WinformsHost(Control renderControl, Panel parent)
            : base(renderControl.Handle, renderControl.Width, renderControl.Height)
        {
            if (renderControl == null) throw new ArgumentNullException("renderControl");

            _renderControl = renderControl;
            _parent = parent;
            

            _mouseWheelPos = 0;

            renderControl.MouseDown += delegate(object sender, System.Windows.Forms.MouseEventArgs args)
            {
                if (MouseButtonDown != null)
                    MouseButtonDown(this, 
                        new MouseEventArgs
                        {
                            Button = XLateButtons(args.Button),
                            Position = XLatePoint(args.Location)
                        });
            };

        

            renderControl.MouseUp += delegate(object sender, System.Windows.Forms.MouseEventArgs args)
            {
                if (MouseButtonUp != null)
                    MouseButtonUp(this,
                        new MouseEventArgs
                        {
                            Button = XLateButtons(args.Button),
                            Position = XLatePoint(args.Location)
                        });
            };

            renderControl.MouseMove += delegate(object sender, System.Windows.Forms.MouseEventArgs args)
            {
                if (MouseMove != null)
                    MouseMove(this,
                        new MouseEventArgs
                        {
                            Button = XLateButtons(args.Button),
                            Position = XLatePoint(args.Location)
                        });
            };

            renderControl.KeyDown += delegate(object sender, System.Windows.Forms.KeyEventArgs args)
            {
                if (KeyDown != null)
                    KeyDown(this,
                        new KeyEventArgs
                        {
                            Alt = args.Alt,
                            Control = args.Control,
                            KeyCode = (KeyCodes) (args.KeyCode)
                        });
            };

            renderControl.KeyUp += delegate(object sender, System.Windows.Forms.KeyEventArgs args)
            {
                if (KeyDown != null)
                    KeyUp(this,
                        new KeyEventArgs
                        {
                            Alt = args.Alt,
                            Control = args.Control,
                            KeyCode = (KeyCodes) (args.KeyCode)
                        });
            };

            renderControl.MouseWheel +=
                delegate(object sender, System.Windows.Forms.MouseEventArgs args) { _mouseWheelPos += args.Delta; };

            renderControl.SizeChanged += delegate
            {
                _renderControl.Invalidate();

                base.Width = _renderControl.Width;
                base.Height = _renderControl.Height;

                DoResize();
                DoRender();
            };

            _keyboard = new WinFormsKeyboardDeviceImp(renderControl);
            _mouse = new WinFormsMouseDeviceImp(renderControl);

            Application.Idle += OnIdle;         

        }

       

        private void OnIdle(object idleSender, EventArgs ea)
        {
            Message message;
            while (!PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
            {
                DoRender();
                
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                Application.Idle -= OnIdle;
                _renderControl = null;
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        public override void SetCursor(CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Standard:
                    _renderControl.Cursor = Cursors.Default;
                    break;
                case CursorType.Hand:
                    _renderControl.Cursor = Cursors.WaitCursor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cursorType");
            }
            
        }

        public override void OpenLink(string link)
        {
            if (link.StartsWith("http://"))
                Process.Start(link);
        }

        public override void Run()
        {
            if (!_initialized)
            {
                DoInit();

                base.Width = _renderControl.Width;
                base.Height = _renderControl.Height;

                DoResize();

                _initialized = true;
            }
        }

        public override int Width
        {
            get { return BaseWidth; }
            set
            {
                BaseWidth = value;
                //_parent.SetSize(BaseWidth, BaseHeight);
            }
        }

        public override int Height
        {
            get { return base.Height; }
            set
            {
                BaseHeight = value;
                //_parent.SetSize(BaseWidth, BaseHeight);
            }
        }

        public override int Top
        {
            get { return BaseTop; }
            set
            {
                BaseTop = value;
                //_parent.SetDesktopLocation(BaseLeft, BaseTop);
            }
        }

        public override int Left
        {
            get { return BaseLeft; }
            set
            {
                BaseLeft = value;
               // _parent.SetDesktopLocation(BaseLeft, BaseTop);
            }
        }

        public void FrameTick(double time)
        {
            // ignore - we create our own timer.
        }

        public void SetMousePos(Point pos)
        {
            throw new NotImplementedException();
        }

        public Point SetMouseToCenter()
        {
            throw new NotImplementedException();
        }

        public bool CursorVisible { get; set; }

        public Point GetMousePos()
        {
            return new Point {x = Cursor.Position.X, y = Cursor.Position.Y};
        }

        public int GetMouseWheelPos()
        {
            return _mouseWheelPos;
        }

        public event EventHandler<MouseEventArgs> MouseButtonDown;
        public event EventHandler<MouseEventArgs> MouseButtonUp;
        public event EventHandler<MouseEventArgs> MouseMove;
        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler<KeyEventArgs> KeyUp;

        private static Fusee.Engine.Common.MouseButtons XLateButtons(System.Windows.Forms.MouseButtons button)
        {
            var result = Fusee.Engine.Common.MouseButtons.Unknown;
            if ((button & System.Windows.Forms.MouseButtons.Left) != 0)
                result |= Fusee.Engine.Common.MouseButtons.Left;
            if ((button & System.Windows.Forms.MouseButtons.Right) != 0)
                result |= Fusee.Engine.Common.MouseButtons.Right;
            if ((button & System.Windows.Forms.MouseButtons.Middle) != 0)
                result |= Fusee.Engine.Common.MouseButtons.Middle;
            return result;
        }

        private static Point XLatePoint(System.Drawing.Point point)
        {
            return new Point {x = point.X, y = point.Y};
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Message
        {
            private readonly IntPtr hWnd;
            private readonly int msg;
            private readonly IntPtr wParam;
            private readonly IntPtr lParam;
            private readonly uint time;
            private readonly Point p;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint
            messageFilterMin, uint messageFilterMax, uint flags);



        /// <summary>
        /// Devices supported by this driver: One mouse and one keyboard.
        /// </summary>
        public IEnumerable<IInputDeviceImp> Devices
        {
            get
            {
                yield return _mouse;
                yield return _keyboard;
            }
        }

        /// <summary>
        /// Returns a human readable description of this driver.
        /// </summary>
        public string DriverDesc
        {
            get
            {
#if PLATFORM_DESKTOP
                const string pf = "Desktop";
#elif PLATFORM_ANDROID
                const string pf = "Android";
#endif
                return "OpenTK GameWindow Mouse and Keyboard input driver for " + pf;
            }
        }

        /// <summary>
        /// Returns a (hopefully) unique ID for this driver. Uniqueness is granted by using the 
        /// full class name (including namespace).
        /// </summary>
        public string DriverId
        {
            get { return GetType().FullName; }
        }

#pragma warning disable 0067
        /// <summary>
        /// Not supported on this driver. Mouse and keyboard are considered to be connected all the time.
        /// You can register handlers but they will never get called.
        /// </summary>
        public event EventHandler<DeviceImpDisconnectedArgs> DeviceDisconnected;

        /// <summary>
        /// Not supported on this driver. Mouse and keyboard are considered to be connected all the time.
        /// You can register handlers but they will never get called.
        /// </summary>
        public event EventHandler<NewDeviceImpConnectedArgs> NewDeviceConnected;
#pragma warning restore 0067

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

 
        #endregion


    }


    /// <summary>
    /// Keyboard input device implementation for Desktop an Android platforms.
    /// </summary>
    public class WinFormsKeyboardDeviceImp : IInputDeviceImp
    {
        private Control _control;
        private Keymapper _keymapper;

        /// <summary>
        /// Should be called by the driver only.
        /// </summary>
        /// <param name="control"></param>
        internal WinFormsKeyboardDeviceImp(Control control)
        {
            _control = control;
            _keymapper = new Keymapper();
            _control.KeyDown += OnGameWinKeyDown;
            _control.KeyUp += OnGameWinKeyUp;

        }

        /// <summary>
        /// Returns the number of Axes (==0, keyboard does not support any axes).
        /// </summary>
        public int AxesCount
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Empty enumeration for keyboard, since <see cref="AxesCount"/> is 0.
        /// </summary>
        public IEnumerable<AxisImpDescription> AxisImpDesc
        {
            get
            {
                yield break;
            }
        }

        /// <summary>
        /// Returns the number of enum values of <see cref="KeyCodes"/>
        /// </summary>
        public int ButtonCount
        {
            get
            {
                return Enum.GetNames(typeof(KeyCodes)).Length;
            }
        }

        /// <summary>
        /// Returns a description for each keyboard button.
        /// </summary>
        public IEnumerable<ButtonImpDescription> ButtonImpDesc
        {
            get
            {
                return from k in _keymapper orderby k.Value.Id select new ButtonImpDescription { ButtonDesc = k.Value, PollButton = false };
            }
        }

        /// <summary>
        /// This is a keyboard device, so this property returns <see cref="DeviceCategory.Keyboard"/>.
        /// </summary>
        public DeviceCategory Category
        {
            get
            {
                return DeviceCategory.Keyboard;
            }
        }

        /// <summary>
        /// Human readable description of this device (to be used in dialogs).
        /// </summary>
        public string Desc
        {
            get
            {
                return "Standard Keyboard implementation.";
            }
        }

        /// <summary>
        /// Returns a (hopefully) unique ID for this driver. Uniqueness is granted by using the 
        /// full class name (including namespace).
        /// </summary>
        public string Id
        {
            get
            {
                return GetType().FullName;
            }
        }


#pragma warning disable 0067
        /// <summary>
        /// No axes exist on this device, so listeners registered to this event will never get called.
        /// </summary>
        public event EventHandler<AxisValueChangedArgs> AxisValueChanged;

        /// <summary>
        /// All buttons exhibited by this device are event-driven buttons, so this is the point to hook to in order
        /// to get information from this device.
        /// </summary>
        public event EventHandler<ButtonValueChangedArgs> ButtonValueChanged;
#pragma warning restore 0067

        /// <summary>
        /// Called when keyboard button is pressed down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="key">The <see cref="KeyboardKeyEventArgs"/> instance containing the event data.</param>
        protected void OnGameWinKeyDown(object sender, System.Windows.Forms.KeyEventArgs keyEventArgs)
        {
            ButtonDescription btnDesc;
            if (ButtonValueChanged != null && _keymapper.TryGetValue(keyEventArgs.KeyCode, out btnDesc))
            {
                ButtonValueChanged(this, new ButtonValueChangedArgs
                {
                    Pressed = true,
                    Button = btnDesc
                });
            }
        }

        /// <summary>
        /// Called when keyboard button is released.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="key">The <see cref="KeyboardKeyEventArgs"/> instance containing the event data.</param>
        protected void OnGameWinKeyUp(object sender, System.Windows.Forms.KeyEventArgs keyEventArgs)
        {
            ButtonDescription btnDesc;
            if (ButtonValueChanged != null && _keymapper.TryGetValue(keyEventArgs.KeyCode, out btnDesc))
            {
                ButtonValueChanged(this, new ButtonValueChangedArgs
                {
                    Pressed = false,
                    Button = btnDesc
                });
            }
        }

        /// <summary>
        /// This device does not support any axes at all. Always throws.
        /// </summary>
        /// <param name="iAxisId">No matter what you specify here, you'll evoke an exception.</param>
        /// <returns>No return, always throws.</returns>
        public float GetAxis(int iAxisId)
        {
            throw new InvalidOperationException($"Unsopported axis {iAxisId}. This device does not support any axis at all.");
        }

        /// <summary>
        /// This device does not support to-be-polled-buttons. All keyboard buttons are event-driven. Listen to the <see cref="ButtonValueChanged"/>
        /// event to reveive keyboard notifications from this device.
        /// </summary>
        /// <param name="iButtonId">No matter what you specify here, you'll evoke an exception.</param>
        /// <returns>No return, always throws.</returns>
        public bool GetButton(int iButtonId)
        {
            throw new InvalidOperationException($"Button {iButtonId} does not exist or is no pollable. Listen to the ButtonValueChanged event to receive keyboard notifications from this device.");
        }
    }

    /// <summary>
    /// Mouse input device implementation for Desktop an Android platforms.
    /// </summary>
    public class WinFormsMouseDeviceImp : IInputDeviceImp
    {
        private Control _form;
        private ButtonImpDescription _btnLeftDesc, _btnRightDesc, _btnMiddleDesc;

        /// <summary>
        /// Creates a new mouse input device instance using an existing <see cref="OpenTK.GameWindow"/>.
        /// </summary>
        /// <param name="gameWindow">The game window providing mouse input.</param>
        public WinFormsMouseDeviceImp(Control gameWindow)
        {
            _form = gameWindow;
            _form.MouseDown += OnGameWinMouseDown;
            _form.MouseUp += OnGameWinMouseUp;

            _btnLeftDesc = new ButtonImpDescription
            {
                ButtonDesc = new ButtonDescription
                {
                    Name = "Left",
                    Id = (int)Fusee.Engine.Common.MouseButtons.Left
                },
                PollButton = false
            };
            _btnMiddleDesc = new ButtonImpDescription
            {
                ButtonDesc = new ButtonDescription
                {
                    Name = "Middle",
                    Id = (int)Fusee.Engine.Common.MouseButtons.Middle
                },
                PollButton = false
            };
            _btnRightDesc = new ButtonImpDescription
            {
                ButtonDesc = new ButtonDescription
                {
                    Name = "Right",
                    Id = (int)Fusee.Engine.Common.MouseButtons.Right
                },
                PollButton = false
            };
        }

        /// <summary>
        /// Number of axes. Here seven: "X", "Y" and "Wheel" as well as MinX, MaxX, MinY and MaxY
        /// </summary>
        public int AxesCount => 7;

        /// <summary>
        /// Returns description information for all axes.
        /// </summary>
        public IEnumerable<AxisImpDescription> AxisImpDesc
        {
            get
            {
                yield return new AxisImpDescription
                {
                    AxisDesc = new AxisDescription
                    {
                        Name = "X",
                        Id = (int)MouseAxes.X,
                        Direction = AxisDirection.X,
                        Nature = AxisNature.Position,
                        Bounded = AxisBoundedType.OtherAxis,
                        MinValueOrAxis = (int)MouseAxes.MinX,
                        MaxValueOrAxis = (int)MouseAxes.MaxX
                    },
                    PollAxis = true
                };
                yield return new AxisImpDescription
                {
                    AxisDesc = new AxisDescription
                    {
                        Name = "Y",
                        Id = (int)MouseAxes.Y,
                        Direction = AxisDirection.Y,
                        Nature = AxisNature.Position,
                        Bounded = AxisBoundedType.OtherAxis,
                        MinValueOrAxis = (int)MouseAxes.MinY,
                        MaxValueOrAxis = (int)MouseAxes.MaxY
                    },
                    PollAxis = true
                };
                yield return new AxisImpDescription
                {
                    AxisDesc = new AxisDescription
                    {
                        Name = "Wheel",
                        Id = (int)MouseAxes.Wheel,
                        Direction = AxisDirection.Z,
                        Nature = AxisNature.Position,
                        Bounded = AxisBoundedType.Unbound,
                        MinValueOrAxis = float.NaN,
                        MaxValueOrAxis = float.NaN
                    },
                    PollAxis = true
                };
                yield return new AxisImpDescription
                {
                    AxisDesc = new AxisDescription
                    {
                        Name = "MinX",
                        Id = (int)MouseAxes.MinX,
                        Direction = AxisDirection.X,
                        Nature = AxisNature.Position,
                        Bounded = AxisBoundedType.Unbound,
                        MinValueOrAxis = float.NaN,
                        MaxValueOrAxis = float.NaN
                    },
                    PollAxis = true
                };
                yield return new AxisImpDescription
                {
                    AxisDesc = new AxisDescription
                    {
                        Name = "MaxX",
                        Id = (int)MouseAxes.MaxX,
                        Direction = AxisDirection.X,
                        Nature = AxisNature.Position,
                        Bounded = AxisBoundedType.Unbound,
                        MinValueOrAxis = float.NaN,
                        MaxValueOrAxis = float.NaN
                    },
                    PollAxis = true
                };
                yield return new AxisImpDescription
                {
                    AxisDesc = new AxisDescription
                    {
                        Name = "MinY",
                        Id = (int)MouseAxes.MinY,
                        Direction = AxisDirection.Y,
                        Nature = AxisNature.Position,
                        Bounded = AxisBoundedType.Unbound,
                        MinValueOrAxis = float.NaN,
                        MaxValueOrAxis = float.NaN
                    },
                    PollAxis = true
                };
                yield return new AxisImpDescription
                {
                    AxisDesc = new AxisDescription
                    {
                        Name = "MaxY",
                        Id = (int)MouseAxes.MaxY,
                        Direction = AxisDirection.Y,
                        Nature = AxisNature.Position,
                        Bounded = AxisBoundedType.Unbound,
                        MinValueOrAxis = float.NaN,
                        MaxValueOrAxis = float.NaN
                    },
                    PollAxis = true
                };
            }
        }

        /// <summary>
        /// Number of buttons exposed by this device. Here three: Left, Middle and Right mouse buttons.
        /// </summary>
        public int ButtonCount => 3;

        /// <summary>
        /// A mouse exposes three buttons: left, middle and right.
        /// </summary>
        public IEnumerable<ButtonImpDescription> ButtonImpDesc
        {
            get
            {
                yield return _btnLeftDesc;
                yield return _btnMiddleDesc;
                yield return _btnRightDesc;
            }
        }

        /// <summary>
        /// Returns <see cref="DeviceCategory.Mouse"/>, just because it's a mouse.
        /// </summary>
        public DeviceCategory Category => DeviceCategory.Mouse;

        /// <summary>
        /// Short description string for this device to be used in dialogs.
        /// </summary>
        public string Desc => "Standard Mouse implementation.";

        /// <summary>
        /// Returns a (hopefully) unique ID for this driver. Uniqueness is granted by using the 
        /// full class name (including namespace).
        /// </summary>
        public string Id => GetType().FullName;

        /// <summary>
        /// No event-based axes are exposed by this device. Use <see cref="GetAxis"/> to akquire mouse axis information.
        /// </summary>
#pragma warning disable 0067
        public event EventHandler<AxisValueChangedArgs> AxisValueChanged;

        /// <summary>
        /// All three mouse buttons are event-based. Listen to this event to get information about mouse button state changes.
        /// </summary>
        public event EventHandler<ButtonValueChangedArgs> ButtonValueChanged;
#pragma warning restore 0067

        /// <summary>
        /// Retrieves values for the X, Y and Wheel axes. No other axes are supported by this device.
        /// </summary>
        /// <param name="iAxisId">The axis to retrieve information for.</param>
        /// <returns>The value at the given axis.</returns>
        public float GetAxis(int iAxisId)
        {
            switch (iAxisId)
            {
                case (int)MouseAxes.X:                  
                    return _form.PointToClient(Cursor.Position).X;
                case (int)MouseAxes.Y:
                    return _form.PointToClient(Cursor.Position).Y;
                case (int)MouseAxes.Wheel:
                    return 0;  // TODO!!!
                case (int)MouseAxes.MinX:
                    return 0;
                case (int)MouseAxes.MaxX:
                    return _form.Width;
                case (int)MouseAxes.MinY:
                    return 0;
                case (int)MouseAxes.MaxY:
                    return _form.Height;
            }
            throw new InvalidOperationException($"Unknown axis {iAxisId}. Cannot get value for unknown axis.");
        }

        /// <summary>
        /// This device does not support to-be-polled-buttons. All mouse buttons are event-driven. Listen to the <see cref="ButtonValueChanged"/>
        /// event to reveive keyboard notifications from this device.
        /// </summary>
        /// <param name="iButtonId">No matter what you specify here, you'll evoke an exception.</param>
        /// <returns>No return, always throws.</returns>
        public bool GetButton(int iButtonId)
        {
            throw new InvalidOperationException(
                $"Unsopported axis {iButtonId}. This device does not support any to-be polled axes at all.");
        }

        /// <summary>
        /// Called when the game window's mouse is pressed down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="mouseArgs">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void OnGameWinMouseDown(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
        {
            if (ButtonValueChanged != null)
            {
                ButtonDescription btnDesc;
                switch (mouseEventArgs.Button)
                {
                    case System.Windows.Forms.MouseButtons.Left:
                        btnDesc = _btnLeftDesc.ButtonDesc;
                        break;
                    case System.Windows.Forms.MouseButtons.Middle:
                        btnDesc = _btnMiddleDesc.ButtonDesc;
                        break;
                    case System.Windows.Forms.MouseButtons.Right:
                        btnDesc = _btnRightDesc.ButtonDesc;
                        break;
                    default:
                        return;
                }

                ButtonValueChanged(this, new ButtonValueChangedArgs
                {
                    Pressed = true,
                    Button = btnDesc
                });
            }
        }

        /// <summary>
        /// Called when the game window's mouse is released.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="mouseArgs">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void OnGameWinMouseUp(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
        {
            if (ButtonValueChanged != null)
            {
                ButtonDescription btnDesc;
                switch (mouseEventArgs.Button)
                {
                    case System.Windows.Forms.MouseButtons.Left:
                        btnDesc = _btnLeftDesc.ButtonDesc;
                        break;
                    case System.Windows.Forms.MouseButtons.Middle:
                        btnDesc = _btnMiddleDesc.ButtonDesc;
                        break;
                    case System.Windows.Forms.MouseButtons.Right:
                        btnDesc = _btnRightDesc.ButtonDesc;
                        break;
                    default:
                        return;
                }

                ButtonValueChanged(this, new ButtonValueChangedArgs
                {
                    Pressed = false,
                    Button = btnDesc
                });
            }
        }
    }
}