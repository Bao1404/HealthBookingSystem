// Doctor Patients functionality
document.addEventListener("DOMContentLoaded", () => {
    initializePatients()
    setupEventListeners()
})

function initializePatients() {
    const doctorName = localStorage.getItem("doctorName") || "Dr. Sarah Johnson"
    document.getElementById("doctorName").textContent = doctorName
}

function createPatientCard(patient) {
    return `
    <div class="patient-card" onclick="viewPatientDetails(${patient.UserId})">
      <div class="patient-card-header">
        <img src="${patient.User.AvatarUrl}" alt="${patient.User.FullName}" class="patient-avatar">
        <div class="patient-info">
          <h6>${patient.User.FullName}</h6>
          <p>${patient.User.Email}</p>
        </div>
      </div>
      <div class="patient-actions" onclick="event.stopPropagation()">
        <button class="btn btn-sm btn-primary" onclick="scheduleAppointment(${patient.UserId})">
          <i class="fas fa-calendar-plus"></i>
        </button>
        <button class="btn btn-sm btn-outline-secondary" onclick="sendMessage(${patient.UserId})">
          <i class="fas fa-envelope"></i>
        </button>
        <button class="btn btn-sm btn-outline-info" onclick="viewMedicalHistory(${patient.UserId})">
          <i class="fas fa-file-medical"></i>
        </button>
      </div>
    </div>
  `
}

function setupEventListeners() {
    // Search functionality
    const searchInput = document.getElementById("searchInput")
    if (searchInput) {
        searchInput.addEventListener("input", (e) => {
            filterPatients(e.target.value)
        })
    }
}

function getStatusColor(status) {
    const colors = {
        active: "success",
        critical: "danger",
        "follow-up": "warning",
        new: "info",
    }
    return colors[status] || "secondary"
}

function formatDate(dateString) {
    const date = new Date(dateString)
    return date.toLocaleDateString("en-US", {
        year: "numeric",
        month: "short",
        day: "numeric",
    })
}

// Action functions
function scheduleAppointment(patientId) {
    console.log("Scheduling appointment for patient:", patientId)
    // Pre-fill patient in appointment modal
    openNewAppointmentModal()
}

function sendMessage(patientId) {
    console.log("Sending message to patient:", patientId)
    window.location.href = `doctor-messages.html?patient=${patientId}`
}

function viewMedicalHistory(patientId) {
    console.log("Viewing medical history for patient:", patientId)
    viewPatientDetails(patientId)
}

function scheduleAppointmentForPatient() {
    // This would be called from the patient details modal
    window.bootstrap.Modal.getInstance(document.getElementById("patientDetailsModal")).hide()
    openNewAppointmentModal()
}

function openAddPatientModal() {
    const modal = window.bootstrap.Modal(document.getElementById("addPatientModal"))
    modal.show()
}

function addPatient() {
    const form = document.getElementById("addPatientForm")
    const formData = new FormData(form)

    console.log("Adding new patient:", Object.fromEntries(formData))

    window.bootstrap.Modal.getInstance(document.getElementById("addPatientModal")).hide()
    showNotification("Patient added successfully!", "success")

    // Reload patients list
    loadPatients()
}

function openNewAppointmentModal() {
    const modal = window.bootstrap.Modal(document.getElementById("newAppointmentModal"))
    modal.show()
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

function logout() {
    if (confirm("Are you sure you want to logout?")) {
        localStorage.clear()
        window.location.href = "login.html"
    }
}
