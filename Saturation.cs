using System;
using HYSYS;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.CompilerServices;

namespace Atoms
{
    public delegate void GetWaterFrac(double data);

    [ComVisible(true)]
    [ProgId("Atoms.Saturation")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]

    public class Saturation
    {
        [DllImport("atoms_saturation_kernel.dll")]
        public static extern void send_info_to_hysys(GetWaterFrac callback);

        private void OnDataReceived(double data)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string extensionFolder = Path.Combine(desktopPath, "ext"); 
            string filePath = Path.Combine(extensionFolder, "info_dump.txt");

            string content = $"Received data from Rust: {data}\n";

            File.AppendAllText(filePath, content);

            waterFraction.SetValue(data);
        }


        [DllImport("atoms_saturation_kernel.dll")]
        public static extern void receive_info_from_hysys(IntPtr value, IntPtr label);

        public void WaterFracFromRust()
        {
            send_info_to_hysys(OnDataReceived);
        }

        private void sendToHysys(string value, string label)
        {
            IntPtr valuePtr = Marshal.StringToHGlobalAnsi(value);
            IntPtr labelPtr = Marshal.StringToHGlobalAnsi(label);
            try
            {
                receive_info_from_hysys(valuePtr, labelPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(valuePtr);
                Marshal.FreeHGlobal(labelPtr);
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
                if (isForgetpass == true) { return; }

                Feed = myContainer.FindVariable("FeedStream").Variable.Object;
                Product = myContainer.FindVariable("ProductStream").Variable.Object;
                eosModel = myContainer.FindVariable("Eos_Model").Variable;
                assocModel = myContainer.FindVariable("Assoc_Model").Variable;
                waterFraction = myContainer.FindVariable("Water_Fraction").Variable;
                crossH2S = myContainer.FindVariable("Cross_H2S_sol").Variable;
                crossCO2 = myContainer.FindVariable("Cross_CO2_sol").Variable;


                if (Feed != null &&
                    Feed.Temperature.IsKnown == true && 
                    Feed.Pressure.IsKnown == true)
                {
                    string temp = Feed.Temperature.GetValue().ToString();
                    
                    sendToHysys(temp, "temp");

                    string pres = Feed.Pressure.GetValue().ToString();
                    sendToHysys(pres, "pres"); 

                    try
                    {
                        dynamic components = Feed.Flowsheet.FluidPackage.Components;
                        string componentNames = string.Join(", ", components.Names);
                        sendToHysys(componentNames, "components");
                    }
                    catch (Exception ex)
                    {
                        sendToHysys(ex.ToString(), "Components exception!");
                    }

                    try
                    {
                        WaterFracFromRust();
                    } catch (Exception ex)
                    {
                        sendToHysys(ex.ToString(), "C# interop exception!");
                    }
                }
            }
            catch (Exception ex) {
                sendToHysys(ex.ToString(), "External exception!");
            }
        }
    }
}
