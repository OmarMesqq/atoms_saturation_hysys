using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HYSYS;
using System.Runtime.InteropServices;

namespace Atoms
{
    //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //public delegate void ReceiveScalarYWDelegate(float scalar);

    public struct VectorY
    {
        public int size;
        public IntPtr data;
    } 

    [ComVisible(true)]
    [ProgId("Atoms.Saturation")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]

    public class Saturation
    {
        [DllImport("dummy_kernel.dll")]
        public static extern int send_scalar_t();
        [DllImport("dummy_kernel.dll")]
        public static extern float send_scalar_p();
        //[DllImport("dummy_kernel.dll")]
        //public static extern VectorY send_vector_y();

        [DllImport("dummy_kernel.dll")]
        public static extern int info_dump(IntPtr str);

        private void LogInfo(string message)
        {
            IntPtr messagePtr = Marshal.StringToHGlobalAnsi(message);
            try
            {
                info_dump(messagePtr);
            }
            finally
            {
                Marshal.FreeHGlobal(messagePtr);
            }
        }

        //public static void ReceiveScalarYW(float scalar)
        //{
        //    ReceiveScalarYWDelegate callback = new ReceiveScalarYWDelegate(ReceiveScalarYW);
        //    IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        //}
        #region: Class Variables declaration
        private HYSYS.ExtnUnitOperationContainer myContainer;
        private HYSYS.ProcessStream Feed;
        private HYSYS.ProcessStream Product;
        const int emptyVal = (int)EmptyValue_enum.HEmpty;
        #endregion

        public int Initialize(HYSYS.ExtnUnitOperationContainer Container, bool IsRecalling)
        {
            myContainer = Container;
            Feed = myContainer.FindVariable("FeedStream").Variable.Object;
            Product = myContainer.FindVariable("ProductStream").Variable.Object;

            return (int)HYSYS.CurrentExtensionVersion_enum.extnCurrentVersion;
        }

        public void Execute(bool isForgetpass)
        {
            try
            {
                if (isForgetpass == true) {return;}
                
                Feed = myContainer.FindVariable("FeedStream").Variable.Object;
                Product = myContainer.FindVariable("ProductStream").Variable.Object;

                if (Feed == null) { return; }
                if (Product == null) { return; }

                int t = send_scalar_t();
                //float p = send_scalar_p();
                //VectorY y = send_vector_y();

                LogInfo("t:");
                LogInfo(t.ToString());

                //LogInfo("p:");
                //LogInfo(p.ToString());

                //LogInfo("y:");
                //LogInfo(((VectorY)y).size.ToString());
                //float[] data = new float[y.size];
                //Marshal.Copy(y.data, data, 0, y.size);

                //for (int i = 0; i < ((VectorY)y).size; i++)
                //{
                //    LogInfo(data[i].ToString());
                //}

                //ReceiveScalarYW(1.0f);
            }
            catch {}
        }

        public void StatusQuery(ObjectStatus Status)
        {
            try
            {
                if (myContainer.ExtensionInterface.IsIgnored == true) { return; }
                if (Feed == null) { Status.AddStatusCondition(HYSYS.StatusLevel_enum.slMissingRequiredInformation, 1, "Requires an Inlet Stream"); }
                if (Product == null) { Status.AddStatusCondition(HYSYS.StatusLevel_enum.slMissingRequiredInformation, 2, "Requires an Outlet Stream"); }
            }
            catch { }
        }
    }
}
