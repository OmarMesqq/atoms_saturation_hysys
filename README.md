### This project simplifies the process of creating a Hysys extension with custom logic. 

The repo's root contains C# code responsible for communicating with Hysys using Windows' COM 
(you need the Aspen Developer Kit for the interop DLL `Interop.HYSYS.1.7.dll`). 

Inside `atoms_saturation_kernel` there is a PC-SAFT modelling logic written in Rust that 
leverages FFI for exchanging the values (such as temperature, pressure etc) between the two 
languages. 

#### Note: receiving a stream's components list
This data structure is more complex than the temperature and pressure of a given stream 
(`double` in C#, which can be "converted" to Rust's `f64`) - it is defined at runtime. 
In other words, only when the user is interacting with the extension in Hysys is this data available.
To access it: 

```csharp
try {
    dynamic components = Feed.Flowsheet.FluidPackage.Components;
    // optionally, get their names in a string
    string componentNames = string.Join(", ", components.Names);
} catch (Exception e) {
    Logger(e.ToString());
}
```
