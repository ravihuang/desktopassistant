using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace activeWindow
{

    public class IPOM
    {
        [DllImport("iphlpapi.dll", EntryPoint = "GetIpAddrTable", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpAddrTable(IntPtr pIpNetTable, [MarshalAs(UnmanagedType.U4)] ref Int32 pdwSize, Boolean bOrder);

        [StructLayout(LayoutKind.Sequential)]
        public struct IpNetRow
        {
            [MarshalAs(UnmanagedType.U4)]
            public Int32 dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public Int32 dwPhysAddrLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] bPhysAddr;
            [MarshalAs(UnmanagedType.U4)]
            public Int32 dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public Int32 dwType;
        } 
        [StructLayout(LayoutKind.Sequential)]
        public struct IPADDRROW
        {
            [MarshalAs(UnmanagedType.U4)]
            public Int32        dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public Int32        dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public Int32        dwMask;
            [MarshalAs(UnmanagedType.U4)]
            public Int32        dwBCastAddr;
            [MarshalAs(UnmanagedType.U4)]
            public Int32        dwReasmSize;
            [MarshalAs(UnmanagedType.U2)]
            public Int16    unused1;
            [MarshalAs(UnmanagedType.U2)]
            public Int16    wType;
        }
        public IPOM() {
            // Call the API
            Int32 bytes = 0;
            Int32 result = GetIpAddrTable(IntPtr.Zero, ref bytes, false);

            IntPtr buffer = IntPtr.Zero;

            try
            {
                // Allocate new memory, make sure we free this at the end.
                buffer = Marshal.AllocCoTaskMem(bytes);
                result = GetIpAddrTable(buffer, ref bytes, false);

                if (result != 0)
                    throw new Exception();

                // Get the length of the buffer
                Int32 rowCount = Marshal.ReadInt32(buffer);

                // Move the buffer
                IntPtr newBuffer = new IntPtr(buffer.ToInt64() + sizeof(Int32));

                // Create the array of rows
                IPADDRROW[] rows = new IPADDRROW[rowCount];

                // Initalize the array
                for (Int32 i = 0; i < rowCount; i++)
                {
                    // Get the structure from the buffer
                    rows[i] = (IPADDRROW)Marshal.PtrToStructure(new IntPtr(newBuffer.ToInt64() + (i * Marshal.SizeOf(typeof(IPADDRROW)))), typeof(IPADDRROW));

                    string ipAddress = new IPAddress(BitConverter.GetBytes(rows[i].dwAddr)).ToString();
                    Console.WriteLine(ipAddress);
                }
            }
            finally
            {
                // Free the allocated memory
                Marshal.FreeCoTaskMem(buffer);
            }            
        
        }

    }
}
