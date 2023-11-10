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
        [DllImport("atoms_saturation_kernel.dll")]
        public static extern void info_dump(IntPtr str);


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
        private HYSYS.InternalRealVariable eosModel;
        private HYSYS.InternalRealVariable assocModel;
        private HYSYS.InternalRealVariable waterFraction;
        private HYSYS.InternalRealVariable crossH2S;
        private HYSYS.InternalRealVariable crossCO2;
        const int emptyVal = (int)EmptyValue_enum.HEmpty;
        #endregion

        public int Initialize(HYSYS.ExtnUnitOperationContainer Container, bool IsRecalling)
        {
            myContainer = Container;
            Feed = myContainer.FindVariable("FeedStream").Variable.Object;
            Product = myContainer.FindVariable("ProductStream").Variable.Object;
            eosModel = myContainer.FindVariable("Eos_Model").Variable;
            assocModel = myContainer.FindVariable("Assoc_Model").Variable;
            waterFraction = myContainer.FindVariable("Water_Fraction").Variable;
            crossH2S = myContainer.FindVariable("Cross_H2S_sol").Variable;
            crossCO2 = myContainer.FindVariable("Cross_CO2_sol").Variable;

            return (int)HYSYS.CurrentExtensionVersion_enum.extnCurrentVersion;
        }

        public void Execute(bool isForgetpass)
        {
            try
            {
                if (isForgetpass == true) { LogInfo("isForgetpass is true"); return; }

                Feed = myContainer.FindVariable("FeedStream").Variable.Object;
                Product = myContainer.FindVariable("ProductStream").Variable.Object;
                eosModel = myContainer.FindVariable("Eos_Model").Variable;
                assocModel = myContainer.FindVariable("Assoc_Model").Variable;
                waterFraction = myContainer.FindVariable("Water_Fraction").Variable;
                crossH2S = myContainer.FindVariable("Cross_H2S_sol").Variable;
                crossCO2 = myContainer.FindVariable("Cross_CO2_sol").Variable;


                if (Feed == null) { LogInfo("Feed stream is null"); return; }

                if (Product == null) { LogInfo("Product stream is null"); return; }

                if (eosModel.Value == emptyVal) { LogInfo("EoS model is empty"); return; }

                if (assocModel.Value == emptyVal) { LogInfo("Assoc model is empty"); return; }
                if (waterFraction.Value == emptyVal) { LogInfo("Water frac is empty"); return; }
                if (crossH2S.Value == emptyVal) { LogInfo("Cross H2S is empty"); return; }
                if (crossCO2.Value == emptyVal) { LogInfo("Cross CO2 is empty"); return; }


                LogInfo("EoS Model:");
                LogInfo(eosModel.Value.ToString());
                LogInfo("Association model:");
                LogInfo(assocModel.Value.ToString());
                LogInfo("Water Fraction so far:");
                LogInfo(waterFraction.Value.ToString());
                LogInfo("Cross H2S model:");
                LogInfo(crossH2S.Value.ToString());
                LogInfo("Cross CO2 model:");
                LogInfo(crossCO2.Value.ToString());
            }
            catch { }
        }
    }
}