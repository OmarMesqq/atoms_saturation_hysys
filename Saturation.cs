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
    [ComVisible(true)]
    [ProgId("Atoms.Saturation")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]

    public class Saturation
    {
        [DllImport("dummy_kernel.dll")]
        public static extern int sendScalarT();
        [DllImport("dummy_kernel.dll")]
        public static extern float sendScalarP();
        [DllImport("dummy_kernel.dll")]
        public static extern void* sendVectorY();

        [DllImport("atoms_saturation_kernel.dll")]
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
        #region: Class Variables declaration
        private HYSYS.ExtnUnitOperationContainer myContainer;
        private HYSYS.ProcessStream Feed;
        private HYSYS.ProcessStream Product;
        const int emptyVal = (int)EmptyValue_enum.HEmpty;
        #endregion

        public int Initialize(HYSYS.ExtnUnitOperationContainer Container, bool IsRecalling)
        {
            LogInfo("Dummy initialized");
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

                int T = sendScalarT();
                float P = sendScalarP();
                void* Y = sendVectorY();

                LogInfo("T:");
                LogInfo(T.ToString());
                LogInfo("P:");
                LogInfo(P.ToString());
                LogInfo("Y:");
                // implement data structure for Y
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
