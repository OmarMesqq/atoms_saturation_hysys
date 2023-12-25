### This project simplifies the process of creating a Hysys extension with custom logic. 

The repo's root contains C# code responsible for communicating with Hysys using Windows' COM 
(you need the Aspen Developer Kit for the interop DLL `Interop.HYSYS.1.7.dll`). 

Inside `atoms_saturation_kernel` there is a PC-SAFT modelling logic written in Rust that 
leverages FFI for exchanging the values (such as temperature, pressure etc) between the two 
languages. 
