// Doctor Schedule JavaScript
let currentWeek = new Date()

document.addEventListener("DOMContentLoaded", () => {
    console.log("DOM Content Loaded - Initializing schedule")
    initializeSchedule()

    // Test modal initialization
    const modalElement = document.getElementById("addTimeOffModal")
    if (modalElement) {
        console.log("Modal element found on page load")

        // Add event listeners for modal close buttons
        const closeButtons = modalElement.querySelectorAll('[data-bs-dismiss="modal"], .btn-close')
        closeButtons.forEach(button => {
            button.addEventListener('click', () => {
                console.log("Modal close button clicked")
                if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                    try {
                        const modal = bootstrap.Modal.getInstance(modalElement)
                        if (modal) {
                            modal.hide()
                        } else {
                            hideModalManually(modalElement)
                        }
                    } catch (error) {
                        console.error("Error closing modal:", error)
                        hideModalManually(modalElement)
                    }
                } else {
                    hideModalManually(modalElement)
                }
            })
        })
    } else {
        console.error("Modal element not found on page load")
    }

    // Test Bootstrap availability
    if (typeof bootstrap !== 'undefined') {
        console.log("Bootstrap is available")
    } else {
        console.error("Bootstrap is not available")
    }
})

function initializeSchedule() {
    loadWeeklySchedule(currentWeek)
}

function getStartOfWeek(date) {
    const d = new Date(date)
    const day = d.getDay()
    const diff = d.getDate() - day
    return new Date(d.setDate(diff))
}

async function loadWeeklySchedule(week) {
    console.log("Loading weekly schedule for week:", week)
    try {
        const url = `/Doctor/GetWeeklySchedule?week=${week.toISOString()}`
        console.log("Fetching from URL:", url)

        const response = await fetch(url)
        console.log("Response status:", response.status)

        const data = await response.json()
        console.log("Response data:", data)

        if (data.success) {
            console.log("Successfully loaded schedule data")
            console.log("Weekly schedule data:", data.weeklySchedule)
            console.log("Current week display:", data.currentWeekDisplay)

            if (data.weeklySchedule && Array.isArray(data.weeklySchedule)) {
                updateScheduleGrid(data.weeklySchedule)
                updateCurrentWeekDisplay(data.currentWeekDisplay)
            } else {
                console.error("Invalid weekly schedule data format")
                showEmptySchedule()
            }
        } else {
            console.error("Failed to load schedule:", data.message)
            showNotification(data.message, "error")
            // Fallback: show empty schedule
            showEmptySchedule()
        }
    } catch (error) {
        console.error("Error loading weekly schedule:", error)
        showNotification("Error loading schedule", "error")
        // Fallback: show empty schedule
        showEmptySchedule()
    }
}

function updateScheduleGrid(weeklySchedule) {
    console.log("Updating schedule grid with data:", weeklySchedule)

    const container = document.getElementById("scheduleGrid")
    if (!container) {
        console.error("Schedule grid container not found")
        return
    }

    const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
    const timeSlots = [
        "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
        "14:00", "14:30", "15:00", "15:30", "16:00", "16:30"
    ]

    let gridHTML = ""

    // Create header row
    gridHTML += '<div class="schedule-grid-header">'
    gridHTML += '<div class="schedule-time-header">Time</div>'
    days.forEach((day) => {
        gridHTML += `<div class="schedule-day-header">${day}</div>`
    })
    gridHTML += "</div>"

    // Create time slot rows
    timeSlots.forEach((time) => {
        gridHTML += '<div class="schedule-grid-row">'
        gridHTML += `<div class="schedule-time-slot">${formatTime(time)}</div>`

        days.forEach((day) => {
            const daySchedule = weeklySchedule.find(d => d.dayName === day)
            console.log(`Day: ${day}, Schedule:`, daySchedule)
            console.log(`Available schedules:`, weeklySchedule.map(d => ({ dayName: d.dayName, appointments: d.appointments?.length || 0 })))

            const appointment = daySchedule?.appointments?.find(a => a.timeDisplay === time)
            const availableSlot = daySchedule?.availableSlots?.find(s => s.timeDisplay.startsWith(time))

            let cellContent = ""
            let cellClass = "schedule-cell"

            if (appointment) {
                console.log(`Found appointment for ${day} at ${time}:`, appointment)
                cellClass += " has-appointment"
                cellContent = `
                    <div class="appointment-block">
                        <div class="appointment-patient">${appointment.patientName}</div>
                        <div class="appointment-type">${appointment.appointmentType}</div>
                        <span class="badge badge-${appointment.statusColor}">${appointment.status}</span>
                    </div>
                `
            } else if (availableSlot?.isAvailable) {
                cellClass += " available"
                cellContent = `<div class="available-slot">Available</div>`
            } else {
                // If no specific data, assume available during working hours
                const hour = parseInt(time.split(':')[0])
                if (hour >= 9 && hour < 17) {
                    cellClass += " available"
                    cellContent = `<div class="available-slot">Available</div>`
                } else {
                    cellClass += " unavailable"
                    cellContent = "<div class='unavailable-slot'>Unavailable</div>"
                }
            }

            gridHTML += `<div class="${cellClass}">${cellContent}</div>`
        })

        gridHTML += "</div>"
    })

    container.innerHTML = gridHTML
    console.log("Schedule grid updated successfully")
}

function updateCurrentWeekDisplay(weekDisplay) {
    const element = document.getElementById("currentWeek")
    if (element) {
        element.textContent = weekDisplay
    }
}

function showEmptySchedule() {
    console.log("Showing empty schedule as fallback")

    // Update week display
    const startOfWeek = getStartOfWeek(currentWeek)
    const endOfWeek = new Date(startOfWeek)
    endOfWeek.setDate(endOfWeek.getDate() + 6)
    const weekDisplay = `${startOfWeek.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${endOfWeek.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}, ${currentWeek.getFullYear()}`
    updateCurrentWeekDisplay(weekDisplay)

    const container = document.getElementById("scheduleGrid")
    if (!container) return

    const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
    const timeSlots = [
        "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
        "14:00", "14:30", "15:00", "15:30", "16:00", "16:30"
    ]

    let gridHTML = ""

    // Create header row
    gridHTML += '<div class="schedule-grid-header">'
    gridHTML += '<div class="schedule-time-header">Time</div>'
    days.forEach((day) => {
        gridHTML += `<div class="schedule-day-header">${day}</div>`
    })
    gridHTML += "</div>"

    // Create time slot rows
    timeSlots.forEach((time) => {
        gridHTML += '<div class="schedule-grid-row">'
        gridHTML += `<div class="schedule-time-slot">${formatTime(time)}</div>`

        days.forEach((day) => {
            // Show all slots as available during working hours
            const hour = parseInt(time.split(':')[0])
            let cellContent = ""
            let cellClass = "schedule-cell"

            if (hour >= 9 && hour < 17) {
                cellClass += " available"
                cellContent = `<div class="available-slot">Available</div>`
            } else {
                cellClass += " unavailable"
                cellContent = "<div class='unavailable-slot'>Unavailable</div>"
            }

            gridHTML += `<div class="${cellClass}">${cellContent}</div>`
        })

        gridHTML += "</div>"
    })

    container.innerHTML = gridHTML
    console.log("Empty schedule displayed")
}

// Utility functions
function formatTime(time24) {
    const [hours, minutes] = time24.split(":")
    const hour = Number.parseInt(hours)
    const ampm = hour >= 12 ? "PM" : "AM"
    const hour12 = hour % 12 || 12
    return `${hour12}:${minutes} ${ampm}`
}

function formatDateRange(startDate, endDate) {
    const start = new Date(startDate)
    const end = new Date(endDate)
    const options = { month: "short", day: "numeric" }

    if (start.getFullYear() === end.getFullYear()) {
        return `${start.toLocaleDateString("en-US", options)} - ${end.toLocaleDateString("en-US", options)}, ${start.getFullYear()}`
    } else {
        return `${start.toLocaleDateString("en-US", { ...options, year: "numeric" })} - ${end.toLocaleDateString("en-US", { ...options, year: "numeric" })}`
    }
}

function getTimeOffIcon(type) {
    const icons = {
        vacation: "plane",
        sick: "thermometer-half",
        conference: "graduation-cap",
        personal: "user",
        holiday: "gift",
    }
    return icons[type] || "calendar-times"
}

// Navigation functions
function previousWeek() {
    currentWeek.setDate(currentWeek.getDate() - 7)
    loadWeeklySchedule(currentWeek)
}

function nextWeek() {
    currentWeek.setDate(currentWeek.getDate() + 7)
    loadWeeklySchedule(currentWeek)
}

// Time Off functions
function addTimeOff() {
    console.log("addTimeOff function called")

    // Reset form for add mode
    document.getElementById("addTimeOffForm").reset()
    document.getElementById("timeOffId").value = "0"
    document.getElementById("saveTimeOffBtn").textContent = "Add Time Off"
    document.querySelector("#addTimeOffModal .modal-title").textContent = "Add Time Off"

    // Show the modal
    const modalElement = document.getElementById("addTimeOffModal")
    console.log("Modal element:", modalElement)

    if (modalElement) {
        // Check if Bootstrap is available
        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            try {
                const modal = new bootstrap.Modal(modalElement)
                modal.show()
                console.log("Modal should be shown")
            } catch (error) {
                console.error("Error creating Bootstrap modal:", error)
                showModalManually(modalElement)
            }
        } else {
            console.log("Bootstrap not available, using manual modal")
            showModalManually(modalElement)
        }
    } else {
        console.error("Modal element not found")
    }
}

function showModalManually(modalElement) {
    // Fallback: show modal manually
    modalElement.style.display = 'block'
    modalElement.classList.add('show')
    document.body.classList.add('modal-open')

    // Add backdrop
    const backdrop = document.createElement('div')
    backdrop.className = 'modal-backdrop fade show'
    backdrop.id = 'manual-backdrop'
    document.body.appendChild(backdrop)

    // Add click handler to backdrop to close modal
    backdrop.addEventListener('click', () => {
        hideModalManually(modalElement)
    })

    console.log("Modal shown manually")
}

function hideModalManually(modalElement) {
    modalElement.style.display = 'none'
    modalElement.classList.remove('show')
    document.body.classList.remove('modal-open')

    // Remove backdrop
    const backdrop = document.getElementById('manual-backdrop')
    if (backdrop) {
        backdrop.remove()
    }
}

function closeTimeOffModal() {
    const modalElement = document.getElementById("addTimeOffModal")
    if (modalElement) {
        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            try {
                const modal = bootstrap.Modal.getInstance(modalElement)
                if (modal) {
                    modal.hide()
                } else {
                    hideModalManually(modalElement)
                }
            } catch (error) {
                console.error("Error closing modal:", error)
                hideModalManually(modalElement)
            }
        } else {
            hideModalManually(modalElement)
        }
    }
}

async function editTimeOff(timeOffId) {
    console.log("editTimeOff function called with ID:", timeOffId)

    try {
        const response = await fetch(`/Doctor/GetTimeOff?id=${timeOffId}`)
        const result = await response.json()

        if (result.success) {
            const timeOff = result.timeOff

            // Fill form with existing data
            document.getElementById("timeOffId").value = timeOff.timeOffId
            document.getElementById("timeOffType").value = timeOff.type
            document.getElementById("timeOffTitle").value = timeOff.title
            document.getElementById("startDate").value = timeOff.startDate
            document.getElementById("endDate").value = timeOff.endDate
            document.getElementById("allDay").checked = timeOff.isAllDay
            document.getElementById("timeOffReason").value = timeOff.reason || ""

            // Update modal title and button
            document.getElementById("saveTimeOffBtn").textContent = "Update Time Off"
            document.querySelector("#addTimeOffModal .modal-title").textContent = "Edit Time Off"

            // Show the modal
            const modalElement = document.getElementById("addTimeOffModal")
            console.log("Modal element for edit:", modalElement)

            if (modalElement) {
                // Check if Bootstrap is available
                if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                    try {
                        const modal = new bootstrap.Modal(modalElement)
                        modal.show()
                        console.log("Edit modal should be shown")
                    } catch (error) {
                        console.error("Error creating Bootstrap modal for edit:", error)
                        showModalManually(modalElement)
                    }
                } else {
                    console.log("Bootstrap not available for edit, using manual modal")
                    showModalManually(modalElement)
                }
            } else {
                console.error("Modal element not found for edit")
            }
        } else {
            showNotification(result.message || "Failed to load time off", "error")
        }
    } catch (error) {
        console.error("Error loading time off:", error)
        showNotification("Error loading time off", "error")
    }
}

async function saveTimeOff() {
    const form = document.getElementById("addTimeOffForm")

    // Get form values directly
    const timeOffId = document.getElementById("timeOffId").value
    const timeOffType = document.getElementById("timeOffType").value
    const timeOffTitle = document.getElementById("timeOffTitle").value
    const startDate = document.getElementById("startDate").value
    const endDate = document.getElementById("endDate").value
    const allDay = document.getElementById("allDay").checked
    const timeOffReason = document.getElementById("timeOffReason").value

    // Validate form
    if (!timeOffType || !timeOffTitle || !startDate || !endDate) {
        showNotification("Please fill in all required fields", "error")
        return
    }

    try {
        const timeOffData = {
            timeOffId: parseInt(timeOffId) || 0,
            type: timeOffType,
            title: timeOffTitle,
            startDate: startDate,
            endDate: endDate,
            isAllDay: allDay,
            reason: timeOffReason
        }

        const url = timeOffData.timeOffId > 0 ? "/Doctor/UpdateTimeOff" : "/Doctor/AddTimeOff"

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(timeOffData)
        })

        const result = await response.json()

        if (result.success) {
            const action = timeOffData.timeOffId > 0 ? "updated" : "added"
            showNotification(`Time off ${action} successfully!`, "success")

            // Hide the modal
            const modalElement = document.getElementById("addTimeOffModal")
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                try {
                    const modal = bootstrap.Modal.getInstance(modalElement)
                    if (modal) {
                        modal.hide()
                    } else {
                        hideModalManually(modalElement)
                    }
                } catch (error) {
                    console.error("Error hiding modal:", error)
                    hideModalManually(modalElement)
                }
            } else {
                hideModalManually(modalElement)
            }

            form.reset()
            // Reload the page to refresh time off list
            location.reload()
        } else {
            showNotification(result.message || `Failed to ${timeOffData.timeOffId > 0 ? 'update' : 'add'} time off`, "error")
        }
    } catch (error) {
        console.error("Error saving time off:", error)
        showNotification("Error saving time off", "error")
    }
}

async function deleteTimeOff(timeOffId) {
    if (confirm("Are you sure you want to delete this time off?")) {
        try {
            const response = await fetch(`/Doctor/DeleteTimeOff?id=${timeOffId}`, {
                method: "POST"
            })

            const result = await response.json()

            if (result.success) {
                showNotification("Time off deleted successfully!", "success")
                // Reload the page to refresh time off list
                location.reload()
            } else {
                showNotification(result.message || "Failed to delete time off", "error")
            }
        } catch (error) {
            console.error("Error deleting time off:", error)
            showNotification("Error deleting time off", "error")
        }
    }
}

function showNotification(message, type = "info") {
    // Simple notification implementation
    const alertClass = type === "error" ? "alert-danger" :
        type === "success" ? "alert-success" : "alert-info"

    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `

    // Add to page or use a notification system
    const container = document.querySelector(".content")
    if (container) {
        const alertDiv = document.createElement("div")
        alertDiv.innerHTML = alertHtml
        container.insertBefore(alertDiv.firstElementChild, container.firstChild)

        // Auto-remove after 5 seconds
        setTimeout(() => {
            const alert = document.querySelector(".alert")
            if (alert) {
                alert.remove()
            }
        }, 5000)
    }
}