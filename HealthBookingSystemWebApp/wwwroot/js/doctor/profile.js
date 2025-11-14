// Doctor Profile JavaScript
document.addEventListener("DOMContentLoaded", () => {
    initializeProfile()
    loadProfileData()
    setupEventListeners()
})

function initializeProfile() {
    const doctorName = localStorage.getItem("doctorName") || "Dr. Sarah Johnson"
    document.getElementById("doctorName").textContent = doctorName
    document.getElementById("profileName").textContent = doctorName
}

function setupEventListeners() {
    // Toggle switches
    const toggles = document.querySelectorAll('input[type="checkbox"]')
    toggles.forEach((toggle) => {
        toggle.addEventListener("change", function () {
            console.log(`${this.id} toggled:`, this.checked)
            // Save setting to localStorage or API
            saveSetting(this.id, this.checked)
        })
    })
}

function saveSetting(settingId, value) {
    const settings = JSON.parse(localStorage.getItem("doctorSettings") || "{}")
    settings[settingId] = value
    localStorage.setItem("doctorSettings", JSON.stringify(settings))
    showNotification("Setting saved successfully", "success")
}

function editProfessionalInfo() {
    const profileData = JSON.parse(localStorage.getItem("doctorProfile") || "{}")

    // Populate edit form
    document.getElementById("editFirstName").value = profileData.firstName || ""
    document.getElementById("editLastName").value = profileData.lastName || ""
    document.getElementById("editSpecialty").value = profileData.specialty?.toLowerCase() || "cardiology"
    document.getElementById("editEmail").value = profileData.email || ""
    document.getElementById("editPhone").value = profileData.phone || ""
    document.getElementById("editLicense").value = profileData.license || ""
    document.getElementById("editMedicalSchool").value = profileData.medicalSchool || ""
    document.getElementById("editHospital").value = profileData.hospital || ""

    // Show modal
    const modal = window.bootstrap.Modal(document.getElementById("editProfessionalModal"))
    modal.show()
}

function saveProfessionalInfo() {
    const form = document.getElementById("editProfessionalForm")
    const formData = new FormData(form)

    const updatedProfile = {
        firstName: document.getElementById("editFirstName").value,
        lastName: document.getElementById("editLastName").value,
        specialty:
            document.getElementById("editSpecialty").options[document.getElementById("editSpecialty").selectedIndex].text,
        email: document.getElementById("editEmail").value,
        phone: document.getElementById("editPhone").value,
        license: document.getElementById("editLicense").value,
        medicalSchool: document.getElementById("editMedicalSchool").value,
        hospital: document.getElementById("editHospital").value,
    }

    // Update display
    const fullName = `Dr. ${updatedProfile.firstName} ${updatedProfile.lastName}`
    document.getElementById("profileName").textContent = fullName
    document.getElementById("doctorName").textContent = fullName
    document.getElementById("profileSpecialty").textContent = updatedProfile.specialty
    document.getElementById("profileEmail").textContent = updatedProfile.email
    document.getElementById("medicalSchool").textContent = updatedProfile.medicalSchool

    // Save to localStorage
    localStorage.setItem("doctorProfile", JSON.stringify(updatedProfile))
    localStorage.setItem("doctorName", fullName)

    // Close modal
    window.bootstrap.Modal.getInstance(document.getElementById("editProfessionalModal")).hide()
    showNotification("Profile updated successfully!", "success")
}

function editSpecializations() {
    console.log("Editing specializations")
    // Implement specializations editing modal
    showNotification("Specializations editing coming soon", "info")
}

function editSchedule() {
    console.log("Editing schedule")
    window.location.href = "doctor-schedule.html"
}

function changeAvatar() {
    const input = document.createElement("input")
    input.type = "file"
    input.accept = "image/*"
    input.onchange = (e) => {
        const file = e.target.files[0]
        if (file) {
            const reader = new FileReader()
            reader.onload = (e) => {
                // Update avatar display
                document.querySelector(".profile-avatar-xl").src = e.target.result
                document.querySelector(".user-avatar").src = e.target.result

                // Save to localStorage (in real app, upload to server)
                localStorage.setItem("doctorAvatar", e.target.result)

                // Upload avatar to server via AJAX
                uploadAvatarToServer(file);

                //showNotification("Avatar updated successfully!", "success")
            }
            reader.readAsDataURL(file)
        }
    }
    input.click()
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
        url: '/updateImageDoctor',  // Đường dẫn đến action trong Controller
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