// Doctor Appointments JavaScript
document.addEventListener("DOMContentLoaded", () => {
    // Only setup event listeners, don't load fake data
    setupEventListeners()
    updatePendingCountFromServer()
})

// Tab event listeners removed - data is loaded from server-side ViewBag

// Data loading functions removed - data comes from server-side ViewBag

// All fake data loading functions removed - data comes from server ViewBag

// Card creation functions removed - HTML comes from server-side Razor

function setupEventListeners() {
    // Search functionality
    const searchInput = document.getElementById("searchInput")
    if (searchInput) {
        searchInput.addEventListener("input", (e) => {
            filterAppointments(e.target.value)
        })
    }
}

function filterAppointments(searchTerm) {
    // Filter appointments based on search term
    const allAppointments = document.querySelectorAll('.appointment-item-detailed')
    const searchLower = searchTerm.toLowerCase()
    
    allAppointments.forEach(appointment => {
        const patientName = appointment.querySelector('h6')?.textContent.toLowerCase() || ''
        const appointmentType = appointment.querySelector('.appointment-type')?.textContent.toLowerCase() || ''
        const notes = appointment.querySelector('strong')?.parentNode?.textContent.toLowerCase() || ''
        
        const isMatch = patientName.includes(searchLower) || 
                       appointmentType.includes(searchLower) || 
                       notes.includes(searchLower)
        
        appointment.style.display = isMatch ? 'block' : 'none'
    })
}

function updatePendingCountFromServer() {
    // Count pending appointments from server-rendered content
    const pendingContainer = document.getElementById("pendingAppointments")
    if (pendingContainer) {
        const pendingCards = pendingContainer.querySelectorAll(".appointment-item-detailed")
        const count = pendingCards.length
        const badge = document.getElementById("pendingCount")
        if (badge) {
            badge.textContent = count
            badge.style.display = count > 0 ? "inline" : "none"
        }
    }
}

// Helper functions for notifications
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
