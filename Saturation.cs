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
        private HYSYS.InternalRealVariable DP;
        private HYSYS.InternalRealVariable Duty;
        const int emptyVal = (int)EmptyValue_enum.HEmpty;

        #endregion
        public int Initialize(HYSYS.ExtnUnitOperationContainer Container, bool IsRecalling)
        {
            LogInfo("Bridge initialized");
            myContainer = Container;
            Feed = myContainer.FindVariable("FeedStream").Variable.Object;
            Product = myContainer.FindVariable("ProductStream").Variable.Object;
            DP = myContainer.FindVariable("DP").Variable;
            Duty = myContainer.FindVariable("Duty").Variable;

            return (int)HYSYS.CurrentExtensionVersion_enum.extnCurrentVersion;

        }

        public void Execute(bool isForgetpass)
        {
            LogInfo("Executing calculations...");
            try
            {
                if (isForgetpass == true) {return;}

                Feed = myContainer.FindVariable("FeedStream").Variable.Object;
                Product = myContainer.FindVariable("ProductStream").Variable.Object;
                DP = myContainer.FindVariable("DP").Variable;
                Duty = myContainer.FindVariable("Duty").Variable;


                //Check that we have enough information to Calculate

                if (Feed == null) { return; }
                if (Product == null) { return; }
                if (DP.Value == emptyVal) { return; }

                int specs = 0;
                if (Feed.Pressure.IsKnown == true || Product.Pressure.IsKnown == true) { specs += 1; }
                if (Duty.IsKnown == true) { specs += 1; }
                if (Feed.Temperature.IsKnown == true || Product.Temperature.IsKnown == true) { specs += 1; }
                if (Feed.VapourFraction.IsKnown == true || Product.VapourFraction.IsKnown == true) { specs += 1; }
                if (Feed.MassFlow.IsKnown == true || Product.MassFlow.IsKnown == true) { specs += 1; }
                if (specs < 3) { return; }

                // DP is mandatory so Pressure is explicit in the flash calculations. Duty is optional, PV-flash, TP-flash, PH-flash and PQ-flash
                // types are supported. The class handles forward and backward calculations.

                //Pressure calculation (mecanical equilibrium)

                if (Feed.Pressure.IsKnown == false && Product.Pressure.IsKnown) //Backward
                {
                    Feed.Pressure.Calculate(Product.Pressure.Value + DP.Value);
                }
                else if (Product.Pressure.IsKnown == false && Feed.Pressure.IsKnown) //Forward
                {
                    Product.Pressure.Calculate(Feed.Pressure.Value - DP.Value);
                }

                // Total and component material balance

                HYSYS.ProcessStream[] StreamList = {Feed, Product};
                myContainer.Balance(HYSYS.BalanceType_enum.btMassBalance, 1, StreamList); // Total material balance
                myContainer.Balance(HYSYS.BalanceType_enum.btMoleBalance, 1, StreamList); // Component material balance


                //Enthalpy balance

                if ((Feed.HeatFlow.IsKnown == true && Product.HeatFlow.IsKnown == true) && Duty.Value == emptyVal)
                {
                    Duty.Calculate(Product.HeatFlow.Value - Feed.HeatFlow.Value);
                }

                if (Duty.Value == emptyVal) { return; } // Duty is optional

                else if (Feed.MolarEnthalpy.IsKnown == false && Product.MolarEnthalpy.IsKnown) //Backward
                {
                    Feed.MolarEnthalpy.Calculate((Product.HeatFlow.Value - Duty.Value)/ Product.MolarFlow.Value);
                }
                else if (Product.MolarEnthalpy.IsKnown == false && Feed.MolarEnthalpy.IsKnown) //Forward
                {
                    Product.MolarEnthalpy.Calculate((Feed.HeatFlow.Value + Duty.Value)/ Feed.MolarFlow.Value);
                }
               
                
                // Check if feed and product streams are fully solved

                if (Feed.DuplicateFluid().IsUpToDate && Product.DuplicateFluid().IsUpToDate)
                {
                    myContainer.SolveComplete();
                }
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
                if (DP.Value == emptyVal) { Status.AddStatusCondition(HYSYS.StatusLevel_enum.slMissingRequiredInformation, 3, "Requires DeltaP specification"); }
                if (Duty.Value == emptyVal && (Feed.Temperature.IsKnown == false || Product.Temperature.IsKnown == false)) { Status.AddStatusCondition(HYSYS.StatusLevel_enum.slMissingRequiredInformation, 4, "Requires Duty specification or stream conditions"); }
            }
            catch { }
        }
    
    }
}
