using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace activeWindow
{
    [Description("窗体自动靠边隐藏完美组件 @Author: Red_angelX")]
    public partial class autoDockManager : Component
    {
        private Form _form;

        public autoDockManager()
        {
            
        }

        public autoDockManager(IContainer container)
        {
            container.Add(this);            
        }

        [Description("用于控制要自动Dock的窗体")]
        public Form dockForm
        {
            get
            {
                return _form;
            }
            set
            {
                _form = value;
                if (_form != null)
                {
                    if (_form.TopMost == false)
                        _form.TopMost = true;

                    if (DesignMode)
                        return;

                    nativeDockWindow dockWindow = new nativeDockWindow();
                    dockWindow.AssignHandle(_form.Handle);
                }
            }
        }
    }

    public class nativeDockWindow : NativeWindow
    {
        IntPtr handle = IntPtr.Zero;
        Form baseForm = null;

        const int HM_NONE = 0;   //不收缩
        const int HM_TOP = 1;   //向上收缩
        const int HM_BOTTOM = 2;   //向下收缩
        const int HM_LEFT = 3;   //向左收缩
        const int HM_RIGHT = 4;   //向右收缩

        const int INTERVAL = 20;  //触发粘附的最小间隔,单位为象素
        const int INFALTE = 4;  //触发收缩的小间隔,单位为象素


        bool m_isWndHide = false;       //窗口是否隐藏
        bool m_isSizeChanged = false;   //窗口大小是否改变了 
        bool m_isSetTimer = false;      //窗口大小是否改变了
        int m_oldWndHeight;    //旧的窗口宽度
        int m_hideMode = HM_NONE;         //隐藏模式

        Timer checkTimer = new Timer();    //定时器

        protected override void OnHandleChange()
        {
            handle = this.Handle;
            if (handle != IntPtr.Zero)
            {
                baseForm = (Form)Form.FromHandle(handle);
                checkTimer.Interval = 200;
                checkTimer.Tick += new EventHandler(checkTimer_Tick);
            }
            base.OnHandleChange();
        }

        //修正移动时窗口的大小
        void FixMoving(ref Message m)
        {
            //已经隐藏了的情况
            if (m_hideMode != HM_NONE && m_isWndHide == true)
            {
                return;
            }

            if (Control.MousePosition.Y <= INTERVAL)
            {   //粘附在上边
                m_hideMode = HM_TOP;
            }
            else if (Control.MousePosition.Y >= (Screen.PrimaryScreen.Bounds.Width - INTERVAL))
            {   //粘附在下边
                m_hideMode = HM_BOTTOM;
            }
            else if (Control.MousePosition.X < INTERVAL)
            {    //粘附在左边    
                if (!m_isSizeChanged)
                {
                    m_oldWndHeight = baseForm.Height;
                }
                m_isSizeChanged = true;
                m_hideMode = HM_LEFT;
            }
            else if (Control.MousePosition.X >= (Screen.PrimaryScreen.Bounds.Width - INTERVAL))
            {   //粘附在右边
                if (!m_isSizeChanged)
                {
                    m_oldWndHeight = baseForm.Height;
                }
                m_isSizeChanged = true;
                m_hideMode = HM_RIGHT;
            }
            else
            {   //不粘附

                if (m_isSizeChanged)
                {   //如果收缩到两边,则拖出来后会变回原来大小
                    //在"拖动不显示窗口内容下"只有光栅变回原来大小
                    int left = Marshal.ReadInt32(m.LParam, sizeof(int) * 0);
                    int top = Marshal.ReadInt32(m.LParam, sizeof(int) * 1);
                    int right = Marshal.ReadInt32(m.LParam, sizeof(int) * 2);
                    Marshal.WriteInt32(m.LParam, sizeof(int) * 0, left); // left
                    Marshal.WriteInt32(m.LParam, sizeof(int) * 1, top); // top
                    Marshal.WriteInt32(m.LParam, sizeof(int) * 2, right); // right
                    Marshal.WriteInt32(m.LParam, sizeof(int) * 3, top + m_oldWndHeight); // bottom
                    m_isSizeChanged = false;
                }
                if (m_isSetTimer)
                {   //如果Timer开启了,则关闭之
                    checkTimer.Stop();
                    m_isSetTimer = false;
                }
                m_isWndHide = false;
                m_hideMode = HM_NONE;
            }
        }

        //从收缩状态显示窗口
        void DoShow()
        {
            if (m_hideMode == HM_NONE || !m_isWndHide)
                return;

            switch (m_hideMode)
            {
                case HM_TOP:
                    baseForm.Location = new Point(baseForm.Location.X, 0);
                    m_isWndHide = false;
                    break;
                case HM_BOTTOM:
                    //不处理
                    //m_isWndHide = false;
                    break;
                case HM_LEFT:
                    baseForm.Location = new Point(0, 1);
                    m_isWndHide = false;
                    break;
                case HM_RIGHT:
                    baseForm.Location = new Point(Screen.PrimaryScreen.Bounds.Width - baseForm.Width, 1);
                    m_isWndHide = false;
                    break;
                default:
                    break;
            }
        }

        //从显示状态收缩窗口
        void DoHide()
        {
            if (m_hideMode == HM_NONE || m_isWndHide)
                return;

            switch (m_hideMode)
            {
                case HM_TOP:
                    //此句必须放在MoveWindow之上,否则大小变成m_wndRect,其他case同样        
                    m_isWndHide = true;
                    baseForm.Location = new Point(baseForm.Location.X, (baseForm.Height - INFALTE) * (-1));
                    break;
                case HM_BOTTOM:
                    //m_isWndHide = true;
                    break;
                case HM_LEFT:
                    m_isWndHide = true;
                    baseForm.Size = new Size(baseForm.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    baseForm.Location = new Point((-1) * (baseForm.Width - INFALTE), 1);
                    break;
                case HM_RIGHT:
                    m_isWndHide = true;
                    baseForm.Size = new Size(baseForm.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    baseForm.Location = new Point(Screen.PrimaryScreen.Bounds.Width - INFALTE, 1);
                    break;
                default:
                    break;
            }
        }


        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOZORDER = 0x0004;
        const UInt32 SWP_NOREDRAW = 0x0008;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const UInt32 SWP_FRAMECHANGED = 0x0020;  // The frame changed: send WM_NCCALCSIZE 
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        const UInt32 SWP_HIDEWINDOW = 0x0080;
        const UInt32 SWP_NOCOPYBITS = 0x0100;
        const UInt32 SWP_NOOWNERZORDER = 0x0200;  // Don't do owner Z ordering 
        const UInt32 SWP_NOSENDCHANGING = 0x0400; // Don't send WM_WINDOWPOSCHANGING 

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
           int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //这个结构在改lparam时要用到
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth,
           int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        static extern bool InflateRect(ref RECT lprc, int dx, int dy);

        [DllImport("user32.dll")]
        static extern bool PtInRect([In] ref RECT lprc, Point pt);


        private void checkTimer_Tick(object sender, EventArgs e)
        {
            RECT tRect;
            //获取此时窗口大小
            GetWindowRect(this.Handle, out tRect);
            //膨胀tRect,以达到鼠标离开窗口边沿一定距离才触发事件
            InflateRect(ref tRect, INFALTE, INFALTE);

            if (!PtInRect(ref tRect, Control.MousePosition))   //如果鼠标离开了这个区域
            {
                checkTimer.Stop();
                m_isSetTimer = false;
                DoHide();  //隐藏
            }
        }

        const int WM_NCHITTEST = 0x84;
        const int WM_MOVING = 0x216;
        const int WM_MOVE = 0x3;
        const int WM_SIZING = 0x214;
        const int HTCAPTION = 0x0002;
        const int WM_EXITSIZEMOVE = 0x232;
        const int WM_ENTERSIZEMOVE = 0x231;
        const int WM_SYSCOMMAND = 0x112;
        const int SC_SIZE = 0xF000;
        const int WM_SETTINGCHANGE = 0x1A;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                if (m_hideMode != HM_NONE && !m_isSetTimer)
                {   //鼠标进入时,如果是从收缩状态到显示状态则开启Timer
                    checkTimer.Start();
                    m_isSetTimer = true;
                    DoShow();
                }
            }
            else if (m.Msg == WM_MOVING)
            {
                FixMoving(ref m);
            }
            else if (m.Msg == WM_ENTERSIZEMOVE)
            {
                baseForm.SuspendLayout();
            }
            else if (m.Msg == WM_EXITSIZEMOVE)
            {
                baseForm.ResumeLayout();
            }
            base.WndProc(ref m);
        }
    }
}