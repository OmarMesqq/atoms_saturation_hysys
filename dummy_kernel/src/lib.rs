use ndarray::Array2;
use std::mem::ManuallyDrop;
use std::os::raw::c_float;

#[repr(C)]
pub struct VectorY {
    data: *const f64,
    size: usize,
}

pub extern "C" fn send_scalar_t() -> i32 {
    let t: i32 = 9999;
    t
}

pub extern "C" fn send_scalar_p() -> f64 {
    let p: f64 = 3.141592653589793238462643383279502884197;
    p
}

pub extern "C" fn send_vector_y() -> VectorY {
    let y = Array2::<f64>::zeros((2, 2));
    let y_ext = VectorY {
        data: y.as_ptr(),
        size: y.len(),
    };
    let _ = ManuallyDrop::new(y);  
    y_ext
}

pub extern "C" fn receive_scalar_yw(callback: extern "C" fn(c_float)) {
    let yw: c_float = 123.0;
    callback(yw);
}
