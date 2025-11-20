document.addEventListener("DOMContentLoaded", () => {
    setupAddDoctorForm();
});

function setupAddDoctorForm() {
    const form = document.getElementById("addDoctor");
    if (!form) return;

    // Lấy tất cả input, select, textarea trong form
    const inputs = form.querySelectorAll("input, select, textarea");

    // 1. GẮN SỰ KIỆN CHO TỪNG Ô INPUT
    inputs.forEach(input => {
        // Sự kiện BLUR: Khi chuột rời khỏi ô input -> Kiểm tra lỗi ngay
        input.addEventListener("blur", validateField);

        // Sự kiện INPUT: Khi đang gõ -> Xóa lỗi đỏ
        input.addEventListener("input", (e) => {
            clearValidation(e.target);

            // Nếu đang gõ vào ô Password -> Cập nhật danh sách yêu cầu (List xanh/đỏ)
            if (e.target.id === "password") {
                updatePasswordRequirements();
            }
            // Nếu đang gõ vào ô Confirm Password -> Kiểm tra khớp mật khẩu ngay
            if (e.target.id === "confirmPassword") {
                validatePasswordMatch();
            }
        });
    });

    // 2. XỬ LÝ SỰ KIỆN SUBMIT FORM
    form.addEventListener("submit", function (e) {
        e.preventDefault(); // Chặn submit mặc định để kiểm tra

        let isFormValid = true;
        let firstErrorField = null;

        // Duyệt qua tất cả các field để validate lại lần cuối
        inputs.forEach(input => {
            // Gọi hàm validateField, nếu trả về false nghĩa là có lỗi
            if (!validateField({ target: input })) {
                isFormValid = false;
                if (!firstErrorField) firstErrorField = input; // Lưu lại ô lỗi đầu tiên
            }
        });

        // Kiểm tra lại logic mật khẩu khớp nhau
        const password = document.getElementById("password");
        const confirm = document.getElementById("confirmPassword");
        if (password && confirm && password.value !== confirm.value) {
            showFieldError(confirm, "Mật khẩu xác nhận không khớp.");
            isFormValid = false;
        }

        // KẾT QUẢ
        if (isFormValid) {
            HTMLFormElement.prototype.submit.call(form);
        } else {
            // Nếu có lỗi -> Cuộn màn hình đến ô lỗi đầu tiên
            if (firstErrorField) {
                firstErrorField.scrollIntoView({ behavior: "smooth", block: "center" });
                firstErrorField.focus();
            }
        }
    });
}

// --- HÀM VALIDATE CHI TIẾT TỪNG FIELD ---
function validateField(event) {
    const field = event.target;
    const value = field.value.trim(); // Lấy giá trị, cắt khoảng trắng thừa
    let isValid = true;

    // Xóa lỗi cũ trước khi kiểm tra
    clearValidation(field);

    // 1. KIỂM TRA BẮT BUỘC (REQUIRED)
    // Nếu thẻ HTML có chữ 'required' và giá trị rỗng
    if (field.hasAttribute("required") && !value) {
        showFieldError(field, "This field is required.");
        return false; // Dừng lại, không cần check các điều kiện khác
    }

    // 2. KIỂM TRA EMAIL
    if (field.name === "email" && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
            showFieldError(field, "Vui lòng nhập địa chỉ email hợp lệ.");
            isValid = false;
        }
    }

    // 3. KIỂM TRA SỐ ĐIỆN THOẠI (VN)
    // Áp dụng cho input có id="phone" hoặc name="phone" hoặc type="tel"
    if ((field.id === "phone" || field.type === "tel") && value) {
        const phoneRegex = /^(0?)(3[2-9]|5[6|8|9]|7[0|6-9]|8[0-6|8|9]|9[0-4|6-9])[0-9]{7}$/;
        if (!phoneRegex.test(value)) {
            showFieldError(field, "Số điện thoại không hợp lệ (VD: 0905xxxxxx).");
            isValid = false;
        }
    }

    // 4. KIỂM TRA MẬT KHẨU (ĐỘ MẠNH)
    if (field.id === "password" && value) {
        // Kiểm tra 4 điều kiện
        const isLength = value.length >= 8;
        const hasUpper = /[A-Z]/.test(value);
        const hasLower = /[a-z]/.test(value);
        const hasNumber = /[\d\W]/.test(value); // Số hoặc ký tự đặc biệt

        if (!isLength || !hasUpper || !hasLower || !hasNumber) {
            showFieldError(field, "Mật khẩu chưa đủ mạnh (xem yêu cầu bên dưới).");
            isValid = false;
            updatePasswordRequirements(); // Cập nhật lại list visual
        }
    }

    // 5. KIỂM TRA CONFIRM PASSWORD
    if (field.id === "confirmPassword" && value) {
        const passwordVal = document.getElementById("password").value;
        if (value !== passwordVal) {
            showFieldError(field, "Mật khẩu không khớp.");
            isValid = false;
        }
    }

    return isValid;
}

// --- CÁC HÀM HỖ TRỢ HIỂN THỊ GIAO DIỆN ---

// Hàm hiện lỗi: Thêm viền đỏ và hiện text thông báo
function showFieldError(field, message) {
    field.classList.add("is-invalid"); // Class của Bootstrap để tô đỏ viền

    // Tìm thẻ div chứa text lỗi (.invalid-feedback)
    let feedback = field.nextElementSibling; // Tìm thằng ngay sau

    // Nếu không thấy (do cấu trúc HTML input-group hoặc khác), tìm trong parent
    if (!feedback || !feedback.classList.contains("invalid-feedback")) {
        feedback = field.parentNode.querySelector(".invalid-feedback");
    }

    if (feedback) {
        feedback.textContent = message;
        feedback.style.display = "block";
    }
}

// Hàm xóa lỗi: Bỏ viền đỏ và ẩn text thông báo
function clearValidation(field) {
    // Xử lý trường hợp truyền vào event thay vì element
    const inputElement = field.target ? field.target : field;

    inputElement.classList.remove("is-invalid");

    let feedback = inputElement.nextElementSibling;
    if (!feedback || !feedback.classList.contains("invalid-feedback")) {
        feedback = inputElement.parentNode.querySelector(".invalid-feedback");
    }

    if (feedback) {
        feedback.style.display = "none";
    }
}

// Hàm so khớp mật khẩu Real-time
function validatePasswordMatch() {
    const password = document.getElementById("password").value;
    const confirmInput = document.getElementById("confirmPassword");

    if (confirmInput.value) {
        if (password !== confirmInput.value) {
            showFieldError(confirmInput, "Mật khẩu không khớp.");
        } else {
            clearValidation(confirmInput);
        }
    }
}

// Hàm cập nhật danh sách yêu cầu mật khẩu (Xanh/Đỏ)
function updatePasswordRequirements() {
    const password = document.getElementById("password").value;

    // Định nghĩa các rules
    const rules = [
        { id: "req-length", valid: password.length >= 8 },
        { id: "req-upper", valid: /[A-Z]/.test(password) },
        { id: "req-lower", valid: /[a-z]/.test(password) },
        { id: "req-number", valid: /[\d\W]/.test(password) }
    ];

    rules.forEach(rule => {
        const item = document.getElementById(rule.id);
        if (item) {
            const icon = item.querySelector("i");

            if (rule.valid) {
                // Đạt yêu cầu -> Màu xanh
                item.classList.add("valid");
                item.classList.remove("invalid");
                if (icon) icon.className = "fas fa-check-circle";
            } else {
                // Chưa đạt -> Màu xám/đỏ
                item.classList.remove("valid");
                item.classList.add("invalid");
                if (icon) icon.className = "fas fa-circle";
            }
        }
    });
}