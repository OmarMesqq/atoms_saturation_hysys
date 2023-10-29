use ndarray::{Array1, Array2, Array3};
use std::os::raw::c_float;
use std::fs::OpenOptions;
use std::io::Write;

/*
TODO: Usar forget ou manually drop?
*/

#[repr(C)]
pub struct VectorY {
    data: *const f64,
    size: i32,
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
    let data = vec![1.0,2.0,3.0,4.0,5.0,6.0,7.0,8.0,9.0];
    //let y = Array1::from_vec(data);
    let y = Array3::from_shape_vec((1,3,3), data).unwrap();
    let y_ext = VectorY {
        data: y.as_ptr(),
        size: y.len() as i32,
    };
    let _ = std::mem::ManuallyDrop::new(y);  
    y_ext
}

#[no_mangle]
pub extern "C" fn receive_scalar_yw(yw: c_float) {
    let foo = yw as f32;
    let writer = || -> std::io::Result<()> {
        let mut file = OpenOptions::new()
        .create(true)  
        .append(true)  
        .write(true)
        .open("rust_info_dump.txt")?;

        if file.metadata()?.len() > 0 {
            match file.write_all(b"\r\n") {
                Ok(()) => println!("Ok"),
                Err(_) => println!("Error")
            }
        }
        
        match file.write_all(b"\n") {
            Ok(()) => println!("Ok"),
            Err(_) => println!("Error")
        }
        file.write_all(foo.to_string().as_bytes())?;
        Ok(())
    }; 
    let _ = writer();

}

#[no_mangle]
pub extern "C" fn info_dump(str: *const libc::c_char) -> () {
    let c_str = unsafe {
        std::ffi::CStr::from_ptr(str)
    };
    let string = c_str.to_str().unwrap();

    let writer = || -> std::io::Result<()> {
        let mut file = OpenOptions::new()
        .create(true)  
        .append(true)  
        .write(true)
        .open("csharp_info_dump.txt")?;

        if file.metadata()?.len() > 0 {
            match file.write_all(b"\r\n") {
                Ok(()) => println!("Ok"),
                Err(_) => println!("Error")
            }
        }
        
        match file.write_all(b"\n") {
            Ok(()) => println!("Ok"),
            Err(_) => println!("Error")
        }
        file.write_all(string.as_bytes())?;
        Ok(())
    }; 
    let _ = writer();   
}
