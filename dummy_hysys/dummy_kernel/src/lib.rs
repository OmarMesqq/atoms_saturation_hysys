use ndarray::Array2;

pub extern "C" fn sendScalarT() -> i32 {
    let T: i32 = 9999;
    T
}

pub extern "C" fn sendScalarP() -> f64 {
    let P: f64 = 3.141592653589793238462643383279502884197;
    P
}

pub extern "C" fn sendVectorY() -> Array2<f64> {
    let Y = Array2::<f64>::zeros((2, 2));
    Y
}

fn receiveScalarYw(){
    let Yw: f64;
    // receive from C#
}