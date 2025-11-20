// Multi-step signup form functionality
let currentStep = 1
const totalSteps = 3
let selectedRole = ""

document.addEventListener("DOMContentLoaded", () => {
    initializeForm()
    setupEventListeners()
    showStep(1) // Đảm bảo step 1 hiện khi load
    updateProgressBar()
})

function initializeForm() {
    // Initialize password strength checker AND requirements list
    const passwordInput = document.getElementById("password")
    if (passwordInput) {
        passwordInput.addEventListener("input", function () {
            checkPasswordStrength()      // Kiểm tra thanh sức mạnh (cũ)
            updatePasswordRequirements() // Kiểm tra danh sách yêu cầu (MỚI)
        })
    }

    // Initialize BMI calculator
    const weightInput = document.getElementById("weight")
    const heightInput = document.getElementById("height")
    const weightUnit = document.getElementById("weightUnit")
    const heightUnit = document.getElementById("heightUnit")

    if (weightInput && heightInput) {
        ;[weightInput, heightInput, weightUnit, heightUnit].forEach((element) => {
            element.addEventListener("input", calculateBMI)
            element.addEventListener("change", calculateBMI)
        })
    }

    // Initialize role selection
    setupRoleSelection()

    // Initialize medical conditions
    setupMedicalConditions()
}

function setupEventListeners() {
    // Form validation on input
    const form = document.getElementById("multiStepSignupForm")
    const inputs = form.querySelectorAll("input, select, textarea")

    inputs.forEach((input) => {
        input.addEventListener("blur", validateField)
        input.addEventListener("input", clearValidation)
    })

    // Password confirmation validation
    const confirmPassword = document.getElementById("confirmPassword")
    if (confirmPassword) {
        confirmPassword.addEventListener("input", validatePasswordMatch)
    }
}

function setupRoleSelection() {
    const roleCards = document.querySelectorAll(".role-card")

    roleCards.forEach((card) => {
        card.addEventListener("click", function () {
            const role = this.dataset.role
            const radioInput = this.querySelector('input[type="radio"]')

            // Remove active class from all cards
            roleCards.forEach((c) => c.classList.remove("active"))

            // Add active class to clicked card
            this.classList.add("active")

            // Check the radio button
            radioInput.checked = true
            selectedRole = role

            // Clear any validation errors
            clearValidation({ target: radioInput })
        })
    })
}

function setupMedicalConditions() {
    const noneCheckbox = document.getElementById("none")
    const otherCheckboxes = document.querySelectorAll('input[name="conditions[]"]:not(#none)')

    if (noneCheckbox) {
        noneCheckbox.addEventListener("change", function () {
            if (this.checked) {
                otherCheckboxes.forEach((checkbox) => {
                    checkbox.checked = false
                })
            }
        })
    }

    otherCheckboxes.forEach((checkbox) => {
        checkbox.addEventListener("change", function () {
            if (this.checked && noneCheckbox) {
                noneCheckbox.checked = false
            }
        })
    })
}

function nextStep() {
    console.log("click");
    if (validateCurrentStep()) {
        if (currentStep < totalSteps) {
            // Special handling for role-based flow
            if (currentStep === 1 && selectedRole === "doctor") {
                // Skip health info step for doctors
                currentStep = 3
                showStep(3)
                updateProgressBar()
                processSignup()
            } else {
                currentStep++
                showStep(currentStep)
                updateProgressBar()

                if (currentStep === 3) {
                    processSignup()
                }
            }
        }
    }
}

function prevStep() {
    if (currentStep > 1) {
        // Special handling for role-based flow
        if (currentStep === 3 && selectedRole === "doctor") {
            // Go back to step 1 for doctors
            currentStep = 1
        } else {
            currentStep--
        }
        showStep(currentStep)
        updateProgressBar()
    }
}

function showStep(step) {
    // Hide all steps
    document.querySelectorAll(".signup-step").forEach((stepEl) => {
        stepEl.classList.remove("active")
    })

    // Show current step
    const currentStepEl = document.getElementById(`step${step}`)
    if (currentStepEl) {
        currentStepEl.classList.add("active")
    }
}

function updateProgressBar() {
    const progressSteps = document.querySelectorAll(".progress-step")

    progressSteps.forEach((step, index) => {
        const stepNumber = index + 1

        if (stepNumber < currentStep || stepNumber === currentStep) {
            step.classList.add("active")
        } else {
            step.classList.remove("active")
        }

        if (stepNumber < currentStep) {
            step.classList.add("completed")
        } else {
            step.classList.remove("completed")
        }
    })
}

function validateCurrentStep() {
    const currentStepEl = document.getElementById(`step${currentStep}`)
    const requiredFields = currentStepEl.querySelectorAll("[required]")
    let isValid = true

    requiredFields.forEach((field) => {
        if (!validateField({ target: field })) {
            isValid = false
        }
    })

    // Additional validation for step 1
    if (currentStep === 1) {
        // Validate password match
        const password = document.getElementById("password").value
        const confirmPassword = document.getElementById("confirmPassword").value

        if (password !== confirmPassword) {
            showFieldError(document.getElementById("confirmPassword"), "Mật khẩu không khớp.")
            isValid = false
        }
    }

    return isValid
}

// --- CẬP NHẬT LOGIC VALIDATE ---
function validateField(event) {
    const field = event.target
    const value = field.value.trim()
    let isValid = true

    // Clear previous validation
    clearValidation(event)

    // Required field validation
    if (field.hasAttribute("required") && !value) {
        showFieldError(field, "Trường này là bắt buộc.")
        isValid = false
    }

    // Email validation
    if (field.type === "email" && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        if (!emailRegex.test(value)) {
            showFieldError(field, "Vui lòng nhập địa chỉ email hợp lệ.")
            isValid = false
        }
    }

    // Password validation (CẬP NHẬT: Kiểm tra tất cả các yêu cầu)
    if (field.id === "password" && value) {
        const lengthCheck = value.length >= 8;
        const upperCheck = /[A-Z]/.test(value);
        const lowerCheck = /[a-z]/.test(value);
        const numberCheck = /[\d\W]/.test(value);

        if (!lengthCheck || !upperCheck || !lowerCheck || !numberCheck) {
            showFieldError(field, "Mật khẩu chưa đủ mạnh. Vui lòng kiểm tra các yêu cầu bên dưới.")
            isValid = false
            // Cập nhật lại list UI để người dùng thấy cái nào thiếu
            updatePasswordRequirements()
        }
    }

    // Phone validation
    if (field.type === "tel" && value) {
        const phoneRegex = /^0(3|5|7|8|9)\d{8}$/;
        if (!phoneRegex.test(value)) {
            showFieldError(field, "Vui lòng nhập số điện thoại Việt Nam hợp lệ (VD: 0905xxxxxx).");
            isValid = false;
        }
    }

    return isValid
}

function clearValidation(event) {
    const field = event.target
    field.classList.remove("is-invalid")

    const feedback = field.parentNode.querySelector(".invalid-feedback")
    if (feedback) {
        feedback.style.display = "none"
    }

    // Clear role selection validation
    if (field.name === "role") {
        const roleContainer = document.querySelector(".role-selection")
        if (roleContainer) roleContainer.classList.remove("is-invalid")
    }
}

function showFieldError(field, message) {
    field.classList.add("is-invalid")

    const feedback = field.parentNode.querySelector(".invalid-feedback")
    if (feedback) {
        feedback.textContent = message
        feedback.style.display = "block"
    }
}

function validatePasswordMatch() {
    const password = document.getElementById("password").value
    const confirmPassword = document.getElementById("confirmPassword").value

    if (confirmPassword && password !== confirmPassword) {
        showFieldError(document.getElementById("confirmPassword"), "Mật khẩu không khớp.")
    } else {
        clearValidation({ target: document.getElementById("confirmPassword") })
    }
}

// --- HÀM MỚI: Cập nhật danh sách yêu cầu mật khẩu ---
function updatePasswordRequirements() {
    const password = document.getElementById("password").value;

    // Các rule kiểm tra
    const requirements = [
        { id: "req-length", valid: password.length >= 8 },
        { id: "req-upper", valid: /[A-Z]/.test(password) },
        { id: "req-lower", valid: /[a-z]/.test(password) },
        { id: "req-number", valid: /[\d\W]/.test(password) } // Số hoặc ký tự đặc biệt
    ];

    requirements.forEach(req => {
        const item = document.getElementById(req.id);
        if (item) {
            const icon = item.querySelector("i");

            if (req.valid) {
                item.classList.add("valid"); // Thêm class xanh
                item.classList.remove("invalid");
                // Đổi icon sang dấu tick (nếu dùng fontawesome)
                if (icon) {
                    icon.className = "fas fa-check-circle";
                }
            } else {
                item.classList.remove("valid");
                item.classList.add("invalid"); // Thêm class xám/đỏ
                // Đổi icon về dấu chấm
                if (icon) {
                    icon.className = "fas fa-circle";
                }
            }
        }
    });
}

function checkPasswordStrength() {
    const password = document.getElementById("password").value
    const strengthBar = document.querySelector(".strength-fill")
    const strengthText = document.querySelector(".strength-text")

    // Nếu không có element thì return để tránh lỗi
    if (!strengthBar || !strengthText) return;

    let strength = 0
    let strengthLabel = "Yếu"
    let strengthColor = "#ef4444"

    // Length check
    if (password.length >= 8) strength += 25

    // Uppercase check
    if (/[A-Z]/.test(password)) strength += 25

    // Lowercase check
    if (/[a-z]/.test(password)) strength += 25

    // Number or special character check
    if (/[\d\W]/.test(password)) strength += 25

    // Determine strength label and color
    if (strength >= 75) {
        strengthLabel = "Mạnh"
        strengthColor = "#10b981"
    } else if (strength >= 50) {
        strengthLabel = "Trung bình"
        strengthColor = "#f59e0b"
    } else if (strength >= 25) {
        strengthLabel = "Khá"
        strengthColor = "#f97316"
    }

    // Update UI
    strengthBar.style.width = `${strength}%`
    strengthBar.style.backgroundColor = strengthColor
    strengthText.textContent = `Độ mạnh mật khẩu: ${strengthLabel}`
    strengthText.style.color = strengthColor
}

function calculateBMI() {
    const weight = Number.parseFloat(document.getElementById("weight").value)
    const height = Number.parseFloat(document.getElementById("height").value)

    // Check if elements exist
    if (!document.getElementById("bmi")) return;

    if (!weight || !height) {
        document.getElementById("bmi").value = ""
        document.getElementById("bmiCategory").textContent = ""
        return
    }

    // Convert to metric units (assuming height is cm)
    let weightKg = weight
    let heightM = height / 100

    // Calculate BMI
    const bmi = weightKg / (heightM * heightM)
    document.getElementById("bmi").value = bmi.toFixed(1)

    // Determine BMI category
    let category = ""
    let categoryClass = ""

    if (bmi < 18.5) {
        category = "Thiếu cân"
        categoryClass = "underweight"
    } else if (bmi < 25) {
        category = "Bình thường"
        categoryClass = "normal"
    } else if (bmi < 30) {
        category = "Thừa cân"
        categoryClass = "overweight"
    } else {
        category = "Béo phì"
        categoryClass = "obese"
    }

    const categoryEl = document.getElementById("bmiCategory")
    if (categoryEl) {
        categoryEl.textContent = category
        categoryEl.className = `bmi-category ${categoryClass}`
    }
}

function togglePassword(fieldId) {
    const field = document.getElementById(fieldId)
    const button = field.parentNode.querySelector(".password-toggle")
    const icon = button.querySelector("i")

    if (field.type === "password") {
        field.type = "text"
        icon.classList.remove("fa-eye")
        icon.classList.add("fa-eye-slash")
    } else {
        field.type = "password"
        icon.classList.remove("fa-eye-slash")
        icon.classList.add("fa-eye")
    }
}

// function processSignup() { ... } (Bạn có thể uncomment phần này nếu cần dùng lại)

function goToDashboard() {
    window.location.href = "dashboard-modern.html"
}

function completeProfile() {
    window.location.href = "profile.html"
}