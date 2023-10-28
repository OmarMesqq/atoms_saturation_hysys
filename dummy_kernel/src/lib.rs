use ndarray::Array2;
use std::mem::ManuallyDrop;
use std::os::raw::c_float;
use std::fs::OpenOptions;
use std::io::Write;

#[repr(C)]
pub struct VectorY {
    data: *const f64,
    size: usize,
}

#[no_mangle]
pub extern "C" fn send_scalar_t() -> i32 {
    let t: i32 = 9999;
    t
}

#[no_mangle]
pub extern "C" fn send_scalar_p() -> f64 {
    let p: f64 = 3.141592653589793238462643383279502884197;
    p
}

#[no_mangle]
pub extern "C" fn send_vector_y() -> VectorY {
    let y = Array2::<f64>::zeros((2, 2));
    let y_ext = VectorY {
        data: y.as_ptr(),
        size: y.len(),
    };
    let _ = ManuallyDrop::new(y);  
    y_ext
}

#[no_mangle]
pub extern "C" fn receive_scalar_yw(callback: extern "C" fn(c_float)) {
    let yw: c_float = 123.0;
    callback(yw);
}

#[no_mangle]
pub extern "C" fn info_dump(str: *const libc::c_char) -> std::io::Result<()> {
    let c_str = unsafe {
        std::ffi::CStr::from_ptr(str)
    };
    let string = c_str.to_str().unwrap();

    let mut file = OpenOptions::new()
        .create(true)  // Create the file if it doesn't exist
        .append(true)  // Append to the file if it already exists
        .write(true)
        .open("info_dump.txt")?;

    if file.metadata()?.len() > 0 {
        file.write_all(b"\r\n");
    }
    file.write_all(b"\n");
    file.write_all(string.as_bytes())?;
    Ok(())
}
