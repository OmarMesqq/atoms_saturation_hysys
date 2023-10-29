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
    [StructLayout(LayoutKind.Sequential)]
    public struct VectorY
    {
        public IntPtr data;
        public int size;
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
        [DllImport("dummy_kernel.dll")]
        public static extern VectorY send_vector_y();

        [DllImport("dummy_kernel.dll")]
        public static extern void info_dump(IntPtr str);

        [DllImport("dummy_kernel.dll")]
        public static extern void receive_scalar_yw(float number);

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
                float p = send_scalar_p();
                VectorY y = send_vector_y();

                LogInfo("t:");
                LogInfo(t.ToString());

                LogInfo("p:");
                LogInfo(p.ToString());

                double[] data = new double[y.size];
                Marshal.Copy(y.data, data,0, y.size);
                LogInfo("Value of the vector Y is:");
                for (int i = 0; i < y.size; i++)
                {
                    LogInfo(data[i].ToString());
                }

                LogInfo("Size of the vector Y is:");
                LogInfo(y.size.ToString());

                float myNum = 2525353.53f;
                receive_scalar_yw(myNum);
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
