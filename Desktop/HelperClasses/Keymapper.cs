using System.Collections.Generic;
using Fusee.Engine.Common;
using System.Windows.Forms;
using Fusee.Engine.Core;

namespace Fusee.Tutorial.Desktop.HelperClasses
{
    internal class Keymapper : Dictionary<Keys, ButtonDescription>
    {
        #region Constructors
        /// <summary>
        /// Initializes the map between KeyCodes and OpenTK.Key
        /// </summary>
        internal Keymapper()
        {
            this.Add(System.Windows.Forms.Keys.Escape, new ButtonDescription {Name = KeyCodes.Escape.ToString(), Id = (int)KeyCodes.Escape});

            // Function keys
            for (int i = 0; i < 24; i++)
            {
                this.Add(System.Windows.Forms.Keys.F1 + i, new ButtonDescription { Name = $"F{i}", Id = (int)KeyCodes.F1 + i });
            }

            // Number keys (0-9)
            for (int i = 0; i <= 9; i++)
            {
                this.Add(System.Windows.Forms.Keys.D0 + i, new ButtonDescription { Name = $"D{i}", Id = (int)0x30 + i } );
            }

            // Letters (A-Z)
            for (int i = 0; i < 26; i++)
            {
                this.Add(System.Windows.Forms.Keys.A + i, new ButtonDescription { Name = ((KeyCodes)(0x41 + i)).ToString(), Id = (int)(0x41 + i) });
            }

            this.Add(System.Windows.Forms.Keys.Tab,  new ButtonDescription {Name = KeyCodes.Tab.ToString(), Id = (int)KeyCodes.Tab});
            this.Add(System.Windows.Forms.Keys.CapsLock,  new ButtonDescription {Name = KeyCodes.Capital.ToString(), Id = (int)KeyCodes.Capital});
            this.Add(System.Windows.Forms.Keys.LControlKey,  new ButtonDescription {Name = KeyCodes.LControl.ToString(), Id = (int)KeyCodes.LControl});
            this.Add(System.Windows.Forms.Keys.LShiftKey,  new ButtonDescription {Name = KeyCodes.LShift.ToString(), Id = (int)KeyCodes.LShift});
            this.Add(System.Windows.Forms.Keys.LWin,  new ButtonDescription {Name = KeyCodes.LWin.ToString(), Id = (int)KeyCodes.LWin});
            this.Add(System.Windows.Forms.Keys.LMenu,  new ButtonDescription {Name = KeyCodes.LMenu.ToString(), Id = (int)KeyCodes.LMenu});
            this.Add(System.Windows.Forms.Keys.Space,  new ButtonDescription {Name = KeyCodes.Space.ToString(), Id = (int)KeyCodes.Space});
            this.Add(System.Windows.Forms.Keys.RMenu,  new ButtonDescription {Name = KeyCodes.RMenu.ToString(), Id = (int)KeyCodes.RMenu});
            this.Add(System.Windows.Forms.Keys.RWin,  new ButtonDescription {Name = KeyCodes.RWin.ToString(), Id = (int)KeyCodes.RWin});
            this.Add(System.Windows.Forms.Keys.Menu,  new ButtonDescription {Name = KeyCodes.Apps.ToString(), Id = (int)KeyCodes.Apps});
            this.Add(System.Windows.Forms.Keys.RControlKey,  new ButtonDescription {Name = KeyCodes.RControl.ToString(), Id = (int)KeyCodes.RControl});
            this.Add(System.Windows.Forms.Keys.RShiftKey,  new ButtonDescription {Name = KeyCodes.RShift.ToString(), Id = (int)KeyCodes.RShift});
            this.Add(System.Windows.Forms.Keys.Enter,  new ButtonDescription {Name = KeyCodes.Return.ToString(), Id = (int)KeyCodes.Return});
            this.Add(System.Windows.Forms.Keys.Back,  new ButtonDescription {Name = KeyCodes.Back.ToString(), Id = (int)KeyCodes.Back});

            this.Add(System.Windows.Forms.Keys.Oem1,  new ButtonDescription {Name = KeyCodes.Oem1.ToString(), Id = (int)KeyCodes.Oem1});
            this.Add(System.Windows.Forms.Keys.Oem2,  new ButtonDescription {Name = KeyCodes.Oem2.ToString(), Id = (int)KeyCodes.Oem2});
            this.Add(System.Windows.Forms.Keys.Oem3,  new ButtonDescription {Name = KeyCodes.Oem3.ToString(), Id = (int)KeyCodes.Oem3});
            this.Add(System.Windows.Forms.Keys.Oem4,  new ButtonDescription {Name = KeyCodes.Oem4.ToString(), Id = (int)KeyCodes.Oem4});
            this.Add(System.Windows.Forms.Keys.Oem5,  new ButtonDescription {Name = KeyCodes.Oem5.ToString(), Id = (int)KeyCodes.Oem5});
            this.Add(System.Windows.Forms.Keys.Oem6,  new ButtonDescription {Name = KeyCodes.Oem6.ToString(), Id = (int)KeyCodes.Oem6});
            this.Add(System.Windows.Forms.Keys.Oem7,  new ButtonDescription {Name = KeyCodes.Oem7.ToString(), Id = (int)KeyCodes.Oem7});
            this.Add(System.Windows.Forms.Keys.Oemplus,  new ButtonDescription {Name = KeyCodes.OemPlus.ToString(), Id = (int)KeyCodes.OemPlus});
            this.Add(System.Windows.Forms.Keys.Oemcomma,  new ButtonDescription {Name = KeyCodes.OemComma.ToString(), Id = (int)KeyCodes.OemComma});
            this.Add(System.Windows.Forms.Keys.OemMinus,  new ButtonDescription {Name = KeyCodes.OemMinus.ToString(), Id = (int)KeyCodes.OemMinus});
            this.Add(System.Windows.Forms.Keys.OemPeriod,  new ButtonDescription {Name = KeyCodes.OemPeriod.ToString(), Id = (int)KeyCodes.OemPeriod});

            this.Add(System.Windows.Forms.Keys.Home,  new ButtonDescription {Name = KeyCodes.Home.ToString(), Id = (int)KeyCodes.Home});
            this.Add(System.Windows.Forms.Keys.End,  new ButtonDescription {Name = KeyCodes.End.ToString(), Id = (int)KeyCodes.End});
            this.Add(System.Windows.Forms.Keys.Delete,  new ButtonDescription {Name = KeyCodes.Delete.ToString(), Id = (int)KeyCodes.Delete});
            this.Add(System.Windows.Forms.Keys.PageUp,  new ButtonDescription {Name = KeyCodes.Prior.ToString(), Id = (int)KeyCodes.Prior});
            this.Add(System.Windows.Forms.Keys.PageDown,  new ButtonDescription {Name = KeyCodes.Next.ToString(), Id = (int)KeyCodes.Next});
            this.Add(System.Windows.Forms.Keys.PrintScreen,  new ButtonDescription {Name = KeyCodes.Print.ToString(), Id = (int)KeyCodes.Print});
            this.Add(System.Windows.Forms.Keys.Pause,  new ButtonDescription {Name = KeyCodes.Pause.ToString(), Id = (int)KeyCodes.Pause});
            this.Add(System.Windows.Forms.Keys.NumLock,  new ButtonDescription {Name = KeyCodes.NumLock.ToString(), Id = (int)KeyCodes.NumLock});

            this.Add(System.Windows.Forms.Keys.Scroll,  new ButtonDescription {Name = KeyCodes.Scroll.ToString(), Id = (int)KeyCodes.Scroll});
            // Do we need to do something here?? this.Add(System.Windows.Forms.Keys.PrintScreen,  new ButtonDescription {Name = KeyCodes.Snapshot.ToString(), Id = (int)KeyCodes.Snapshot});
            this.Add(System.Windows.Forms.Keys.Clear,  new ButtonDescription {Name = KeyCodes.Clear.ToString(), Id = (int)KeyCodes.Clear});
            this.Add(System.Windows.Forms.Keys.Insert,  new ButtonDescription {Name = KeyCodes.Insert.ToString(), Id = (int)KeyCodes.Insert});

            this.Add(System.Windows.Forms.Keys.Sleep,  new ButtonDescription {Name = KeyCodes.Sleep.ToString(), Id = (int)KeyCodes.Sleep});

            // Keypad
            for (int i = 0; i <= 9; i++)
            {
                this.Add(System.Windows.Forms.Keys.NumPad0 + i, new ButtonDescription { Name = $"Numpad{i}", Id = (int)KeyCodes.NumPad0 + i});
            }

            this.Add(System.Windows.Forms.Keys.Decimal,  new ButtonDescription {Name = KeyCodes.Decimal.ToString(), Id = (int)KeyCodes.Decimal});
            this.Add(System.Windows.Forms.Keys.Add,  new ButtonDescription {Name = KeyCodes.Add.ToString(), Id = (int)KeyCodes.Add});
            this.Add(System.Windows.Forms.Keys.Subtract,  new ButtonDescription {Name = KeyCodes.Subtract.ToString(), Id = (int)KeyCodes.Subtract});
            this.Add(System.Windows.Forms.Keys.Divide,  new ButtonDescription {Name = KeyCodes.Divide.ToString(), Id = (int)KeyCodes.Divide});
            this.Add(System.Windows.Forms.Keys.Multiply,  new ButtonDescription {Name = KeyCodes.Multiply.ToString(), Id = (int)KeyCodes.Multiply});

            // Navigation
            this.Add(System.Windows.Forms.Keys.Up,  new ButtonDescription {Name = KeyCodes.Up.ToString(), Id = (int)KeyCodes.Up});
            this.Add(System.Windows.Forms.Keys.Down,  new ButtonDescription {Name = KeyCodes.Down.ToString(), Id = (int)KeyCodes.Down});
            this.Add(System.Windows.Forms.Keys.Left,  new ButtonDescription {Name = KeyCodes.Left.ToString(), Id = (int)KeyCodes.Left});
            this.Add(System.Windows.Forms.Keys.Right,  new ButtonDescription {Name = KeyCodes.Right.ToString(), Id = (int)KeyCodes.Right});
            /*
            catch (ArgumentException e)
            {
                //Debug.Print("Exception while creating keymap: '{0}'.", e.ToString());
                System.Windows.Forms.MessageBox.Show(
                    String.Format("Exception while creating keymap: '{0}'.", e.ToString()));
            }
           */
        }
        #endregion
    }
}
