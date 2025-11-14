// Profile page functionality
document.addEventListener("DOMContentLoaded", () => {
    updateUserInfo()
})

function updateUserInfo() {
    const userName = localStorage.getItem("userName") || "John Doe"
    const userEmail = localStorage.getItem("userEmail") || "john.doe@email.com"

    document.getElementById("userName").textContent = userName
    document.getElementById("profileName").textContent = userName
    document.getElementById("profileEmail").textContent = userEmail
}

function editPersonalInfo() {
    const modal = new window.bootstrap.Modal(document.getElementById("editPersonalModal"))

    // Pre-fill form with current data
    const userName = localStorage.getItem("userName") || "John Doe"
    const userEmail = localStorage.getItem("userEmail") || "john.doe@email.com"
    const [firstName, lastName] = userName.split(" ")

    document.getElementById("editFirstName").value = firstName || ""
    document.getElementById("editLastName").value = lastName || ""
    document.getElementById("editEmail").value = userEmail
    document.getElementById("editPhone").value = "+1 (555) 123-4567"
    document.getElementById("editDOB").value = "1990-01-01"
    document.getElementById("editGender").value = "male"
    document.getElementById("editAddress").value = "123 Main St, City, State 12345"
    document.getElementById("editEmergencyContact").value = "Jane Doe - (555) 987-6543"

    modal.show()
}

function savePersonalInfo() {
    const firstName = document.getElementById("editFirstName").value
    const lastName = document.getElementById("editLastName").value
    const email = document.getElementById("editEmail").value
    const phone = document.getElementById("editPhone").value
    const dob = document.getElementById("editDOB").value

    // Update localStorage
    localStorage.setItem("userName", `${firstName} ${lastName}`)
    localStorage.setItem("userEmail", email)

    // Update UI
    updateUserInfo()
    document.getElementById("profilePhone").textContent = phone
    document.getElementById("profileDOB").textContent = new Date(dob).toLocaleDateString("en-US", {
        year: "numeric",
        month: "long",
        day: "numeric",
    })

    // Close modal
    const modal = window.bootstrap.Modal.getInstance(document.getElementById("editPersonalModal"))
    modal.hide()

    alert("Personal information updated successfully!")
}

function editHealthInfo() {
    alert("Health information editing would be implemented here")
}

function editInsurance() {
    alert("Insurance information editing would be implemented here")
}

function changeAvatar() {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "image/*";

    input.onchange = (e) => {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                // Update avatar display
                document.querySelector(".profile-avatar-xl").src = e.target.result;
                document.querySelector(".user-avatar").src = e.target.result;

                // Save to localStorage (in real app, upload to server)
                localStorage.setItem("doctorAvatar", e.target.result);

                // Show notification
                showNotification("Avatar updated successfully!", "success");

                // Upload avatar to server via AJAX
                uploadAvatarToServer(file);
            };
            reader.readAsDataURL(file);
        }
    };

    input.click();
}

function uploadAvatarToServer(file) {
    const formData = new FormData();
    formData.append("avatar", file);

    // Gọi AJAX để upload ảnh lên server
    $.ajax({
        url: '/updateImagePatient',  // Đường dẫn đến action trong Controller
        type: 'POST',
        data: formData,
        processData: false,  // Không chuyển đổi dữ liệu (FormData)
        contentType: false,  // Để trình duyệt tự động xử lý kiểu content-type
        success: function (response) {
            // Xử lý sau khi upload thành công
            if (response.success) {
                showNotification(response.message, "success");
            } else {
                showNotification(response.message, "error");
            }
        },
        error: function (xhr, status, error) {
            showNotification("Avatar update error!", "error");
        }
    });
}


function showNotification(message, type = "info") {
    const notification = document.createElement("div")
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`
    notification.style.cssText = "top: 20px; right: 20px; z-index: 9999;"
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `
    document.body.appendChild(notification)

    setTimeout(() => {
        notification.remove()
    }, 3000)
}


function addAllergy() {
    const allergy = prompt("Enter new allergy:")
    if (allergy) {
        alert(`Added allergy: ${allergy}`)
    }
}

function addMedication() {
    const medication = prompt("Enter new medication:")
    if (medication) {
        alert(`Added medication: ${medication}`)
    }
}

function addCondition() {
    const condition = prompt("Enter new medical condition:")
    if (condition) {
        alert(`Added condition: ${condition}`)
    }
}

function deleteAccount() {
    if (confirm("Are you sure you want to delete your account? This action cannot be undone.")) {
        if (confirm("This will permanently delete all your data. Are you absolutely sure?")) {
            alert("Account deletion would be processed here")
        }
    }
}