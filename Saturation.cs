using System;
using HYSYS;
using System.Runtime.InteropServices;
using System.IO;

namespace Atoms
{
    public delegate void WaterFraction(double data);

    [ComVisible(true)]
    [ProgId("Atoms.Saturation")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class Saturation
    {
        private void Logger(string message)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string extensionFolder = Path.Combine(desktopPath, "ext");
            string filePath = Path.Combine(extensionFolder, "Logger_log.txt");

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string content = $"{timestamp}: {message}\n";

            File.AppendAllText(filePath, content);
        }


        [DllImport("atoms_saturation_kernel.dll")]
        public static extern void send_info_to_hysys(WaterFraction callback);

        private void OnDataReceived(double data)
        {
            string message = $"Received data from Rust: {data}\n";
            Logger(message);
            waterFraction.SetValue(data);
        }


        [DllImport("atoms_saturation_kernel.dll")]
        public static extern void receive_info_from_hysys(IntPtr value, IntPtr label);

        public void GetWaterFraction()
        {
            send_info_to_hysys(OnDataReceived);
        }

        private void SendToHysys(string temperature, string pressure)
        {
            IntPtr temperaturePtr = Marshal.StringToHGlobalAnsi(temperature);
            IntPtr pressurePtr = Marshal.StringToHGlobalAnsi(pressure);
            try
            {
                receive_info_from_hysys(temperaturePtr, pressurePtr);
            }
            finally
            {
                Marshal.FreeHGlobal(temperaturePtr);
                Marshal.FreeHGlobal(pressurePtr);
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
                if (isForgetpass) return;

                Feed = myContainer.FindVariable("FeedStream").Variable.Object;
                Product = myContainer.FindVariable("ProductStream").Variable.Object;
                eosModel = myContainer.FindVariable("Eos_Model").Variable;
                assocModel = myContainer.FindVariable("Assoc_Model").Variable;
                waterFraction = myContainer.FindVariable("Water_Fraction").Variable;
                crossH2S = myContainer.FindVariable("Cross_H2S_sol").Variable;
                crossCO2 = myContainer.FindVariable("Cross_CO2_sol").Variable;


                if (Feed != null &&
                    Feed.Temperature.IsKnown &&
                    Feed.Pressure.IsKnown)
                {
                    double temp = Feed.Temperature.GetValue();
                    double pres = Feed.Pressure.GetValue();

                    if (temp == 0 || pres == 0) return;

                    SendToHysys(temp.ToString(), pres.ToString());

                    try
                    {
                        GetWaterFraction();
                    }
                    catch (Exception ex)
                    {
                        Logger(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger(ex.ToString());
            }
        }
    }
}
