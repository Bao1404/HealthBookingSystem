// Admin Dashboard functionality
let appointmentChart, patientStatusChart, appointmentOverviewChart
const Chart = window.Chart

document.addEventListener("DOMContentLoaded", () => {
    initializeCharts()
    loadAppointmentStatistics()
    loadRecentPatients()
    loadSystemAlerts()
    updateStatistics()
})

function initializeCharts() {
    // Appointment Trends Chart
    const appointmentCtx = document.getElementById("appointmentChart")
    if (appointmentCtx) {
        appointmentChart = new Chart(appointmentCtx.getContext("2d"), {
            type: "line",
            data: {
                labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                datasets: [
                    {
                        label: "Appointments",
                        data: [45, 52, 48, 55, 62, 58, 65, 72, 68, 75, 82, 78],
                        borderColor: "#3b82f6",
                        backgroundColor: "rgba(59, 130, 246, 0.1)",
                        tension: 0.4,
                        fill: true,
                    },
                ],
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false,
                    },
                    tooltip: {
                        callbacks: {
                            title: function(context) {
                                const dataIndex = context[0].dataIndex
                                const currentPeriod = window.currentAppointmentPeriod || 'monthly'
                                
                                if (currentPeriod === 'daily' && window.appointmentData) {
                                    return window.appointmentData.dailyAppointmentTrends[dataIndex]?.date || context[0].label
                                } else if (currentPeriod === 'weekly' && window.appointmentData) {
                                    return window.appointmentData.weeklyAppointmentTrends[dataIndex]?.period || context[0].label
                                }
                                return context[0].label
                            },
                            label: function(context) {
                                return `Appointments: ${context.parsed.y}`
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: "#e2e8f0",
                        },
                    },
                    x: {
                        grid: {
                            color: "#e2e8f0",
                        },
                    },
                },
            },
        })
    }

    // Patient Status Chart
    const patientStatusCtx = document.getElementById("patientStatusChart")
    if (patientStatusCtx) {
        patientStatusChart = new Chart(patientStatusCtx.getContext("2d"), {
            type: "doughnut",
            data: {
                labels: ["Active", "Inactive"],
                datasets: [
                    {
                        data: [85, 15],
                        backgroundColor: ["#10b981", "#6b7280"],
                        borderWidth: 0,
                    },
                ],
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: "bottom",
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                        },
                    },
                },
            },
        })
    }

    // Appointment Overview Chart
    const appointmentOverviewCtx = document.getElementById("appointmentOverviewChart")
    if (appointmentOverviewCtx) {
        appointmentOverviewChart = new Chart(appointmentOverviewCtx.getContext("2d"), {
            type: "bar",
            data: {
                labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                datasets: [
                    {
                        label: "Confirmed",
                        data: [45, 52, 48, 61, 55, 67, 73, 69, 78, 85, 89, 94],
                        backgroundColor: "rgba(16, 185, 129, 0.8)",
                        borderColor: "#10b981",
                        borderWidth: 1,
                        borderRadius: 4,
                    },
                    {
                        label: "Pending",
                        data: [12, 15, 18, 22, 19, 25, 28, 24, 30, 35, 32, 38],
                        backgroundColor: "rgba(245, 158, 11, 0.8)",
                        borderColor: "#f59e0b",
                        borderWidth: 1,
                        borderRadius: 4,
                    },
                ],
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: "top",
                    },
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: "#e2e8f0",
                        },
                        ticks: {
                            callback: (value) => value + " appointments",
                        },
                    },
                    x: {
                        grid: {
                            display: false,
                        },
                    },
                },
            },
        })
    }
}

function updateAppointmentChart(period) {
    if (appointmentChart && window.appointmentData) {
        let newData, newLabels
        
        switch (period) {
            case "daily":
                newData = window.appointmentData.dailyAppointmentTrends.map(t => t.count)
                newLabels = window.appointmentData.dailyAppointmentTrends.map(t => t.day)
                break
            case "weekly":
                newData = window.appointmentData.weeklyAppointmentTrends.map(t => t.count)
                newLabels = window.appointmentData.weeklyAppointmentTrends.map(t => t.week)
                break
            case "monthly":
                newData = window.appointmentData.appointmentTrends.map(t => t.count)
                newLabels = window.appointmentData.appointmentTrends.map(t => t.month)
                break
            default:
                return
        }
        
        appointmentChart.data.labels = newLabels
        appointmentChart.data.datasets[0].data = newData
        appointmentChart.update()
        
        // Track current period for tooltips
        window.currentAppointmentPeriod = period
        
        // Calculate and update period statistics
        updatePeriodStats(newData)
        
        // Update the dropdown button text
        const dropdownButton = document.querySelector('.card-header .dropdown-toggle')
        if (dropdownButton) {
            switch (period) {
                case "daily":
                    dropdownButton.textContent = "Last 7 Days"
                    break
                case "weekly":
                    dropdownButton.textContent = "Last 12 Weeks"
                    break
                case "monthly":
                    dropdownButton.textContent = "Last 12 Months"
                    break
            }
        }
    }
}

function updatePatientStatusChart() {
    if (patientStatusChart) {
        // Switch back to status distribution
        patientStatusChart.data.labels = ["Active", "Inactive"]
        patientStatusChart.data.datasets[0].data = [85, 15]
        patientStatusChart.data.datasets[0].backgroundColor = ["#10b981", "#6b7280"]
        patientStatusChart.update()
    }
}

function updatePatientAgeChart() {
    if (patientStatusChart) {
        // Switch to age distribution
        patientStatusChart.data.labels = ["18-25", "26-35", "36-45", "46-55", "56-65", "65+"]
        patientStatusChart.data.datasets[0].data = [25, 35, 28, 20, 15, 12]
        patientStatusChart.data.datasets[0].backgroundColor = ["#3b82f6", "#8b5cf6", "#06b6d4", "#10b981", "#f59e0b", "#ef4444"]
        patientStatusChart.update()
    }
}

function updatePeriodStats(data) {
    if (!data || data.length === 0) return
    
    const total = data.reduce((sum, value) => sum + value, 0)
    const average = Math.round((total / data.length) * 10) / 10
    const peak = Math.max(...data)
    
    document.getElementById('periodTotal').textContent = total
    document.getElementById('periodAverage').textContent = average
    document.getElementById('periodPeak').textContent = peak
}

function updateAppointmentOverviewChart(period) {
    let confirmedData, pendingData, newLabels

    // Use real data if available, otherwise fall back to mock data
    if (window.appointmentData && window.appointmentData.appointmentOverviewData) {
        switch (period) {
            case "daily":
                // For daily view, we'll use the last 7 days from the appointment data
                const last7Days = window.appointmentData.dailyAppointmentTrends || []
                confirmedData = last7Days.map(t => Math.floor(t.count * 0.7)) // Estimate 70% confirmed
                pendingData = last7Days.map(t => Math.floor(t.count * 0.3))   // Estimate 30% pending
                newLabels = last7Days.map(t => t.day)
                break
            case "weekly":
                // For weekly view, we'll use the weekly trends data
                const weeklyData = window.appointmentData.weeklyAppointmentTrends || []
                confirmedData = weeklyData.map(t => Math.floor(t.count * 0.7))
                pendingData = weeklyData.map(t => Math.floor(t.count * 0.3))
                newLabels = weeklyData.map(t => t.week)
                break
            case "monthly":
                // Use the real monthly data from database
                confirmedData = window.appointmentData.appointmentOverviewData.map(t => t.confirmed)
                pendingData = window.appointmentData.appointmentOverviewData.map(t => t.pending)
                newLabels = window.appointmentData.appointmentOverviewData.map(t => t.month)
                break
        }
    } else {
        // Fallback to mock data if real data is not available
        switch (period) {
            case "daily":
                confirmedData = [8, 12, 9, 15, 11, 13, 16]
                pendingData = [3, 5, 4, 7, 6, 8, 9]
                newLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"]
                break
            case "weekly":
                confirmedData = [45, 52, 48, 61, 55, 67, 73, 69, 78, 85, 89, 94]
                pendingData = [12, 15, 18, 22, 19, 25, 28, 24, 30, 35, 32, 38]
                newLabels = ["Week 1", "Week 2", "Week 3", "Week 4", "Week 5", "Week 6", "Week 7", "Week 8", "Week 9", "Week 10", "Week 11", "Week 12"]
                break
            case "monthly":
                confirmedData = [45, 52, 48, 61, 55, 67, 73, 69, 78, 85, 89, 94]
                pendingData = [12, 15, 18, 22, 19, 25, 28, 24, 30, 35, 32, 38]
                newLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
                break
        }
    }

    appointmentOverviewChart.data.datasets[0].data = confirmedData
    appointmentOverviewChart.data.datasets[1].data = pendingData
    appointmentOverviewChart.data.labels = newLabels
    appointmentOverviewChart.update()

    // Update button states
    document.querySelectorAll(".card-header .btn").forEach((btn) => {
        btn.classList.remove("btn-primary")
        btn.classList.add("btn-outline-primary")
    })
    event.target.classList.remove("btn-outline-primary")
    event.target.classList.add("btn-primary")
}

async function loadAppointmentStatistics() {
    try {
        const response = await fetch('/Admin/GetDashboardStatistics')
        if (response.ok) {
            const data = await response.json()
            
            // Update statistics cards
            document.getElementById('totalPatients').textContent = data.totalPatients
            document.getElementById('activePatients').textContent = data.activePatients
            document.getElementById('totalAppointments').textContent = data.totalAppointments
            document.getElementById('confirmedAppointments').textContent = data.confirmedAppointments
            document.getElementById('pendingAppointments').textContent = data.pendingAppointments
            
            // Update growth indicators
            const patientGrowth = document.getElementById('patientGrowth')
            const newPatients = document.getElementById('newPatients')
            const confirmedGrowth = document.getElementById('confirmedGrowth')
            const pendingGrowth = document.getElementById('pendingGrowth')
            
            if (data.newPatientsThisMonth > 0) {
                patientGrowth.textContent = `+${data.newPatientsThisMonth} this month`
                patientGrowth.className = 'text-success'
            } else {
                patientGrowth.textContent = 'No new patients this month'
                patientGrowth.className = 'text-muted'
            }
            
            if (data.newPatientsThisWeek > 0) {
                newPatients.textContent = `+${data.newPatientsThisWeek} this week`
                newPatients.className = 'text-info'
            } else {
                newPatients.textContent = 'No new patients this week'
                newPatients.className = 'text-muted'
            }
            
            // Update appointment growth indicators
            if (data.confirmedAppointments > 0) {
                confirmedGrowth.textContent = `${data.confirmedAppointments} confirmed`
                confirmedGrowth.className = 'text-success'
            } else {
                confirmedGrowth.textContent = 'No confirmed appointments'
                confirmedGrowth.className = 'text-muted'
            }
            
            if (data.pendingAppointments > 0) {
                pendingGrowth.textContent = `${data.pendingAppointments} pending`
                pendingGrowth.className = 'text-warning'
            } else {
                pendingGrowth.textContent = 'No pending appointments'
                pendingGrowth.className = 'text-muted'
            }
            
            // Update appointment trends chart
            if (appointmentChart) {
                appointmentChart.data.labels = data.appointmentTrends.map(t => t.month)
                appointmentChart.data.datasets[0].data = data.appointmentTrends.map(t => t.count)
                appointmentChart.update()
            }
            
            // Update appointment overview chart with real data
            if (appointmentOverviewChart && data.appointmentOverviewData) {
                appointmentOverviewChart.data.labels = data.appointmentOverviewData.map(t => t.month)
                appointmentOverviewChart.data.datasets[0].data = data.appointmentOverviewData.map(t => t.confirmed)
                appointmentOverviewChart.data.datasets[1].data = data.appointmentOverviewData.map(t => t.pending)
                appointmentOverviewChart.update()
            }
            
            // Store the trend data for chart updates
            window.appointmentData = data
            
            // Initialize period stats for monthly view
            updatePeriodStats(data.appointmentTrends.map(t => t.count))
            window.currentAppointmentPeriod = 'monthly'
            
            // Update patient status chart
            if (patientStatusChart) {
                patientStatusChart.data.datasets[0].data = data.statusDistribution.map(s => s.count)
                patientStatusChart.update()
            }
            
            // Update recent patients list
            if (data.recentPatients) {
                updateRecentPatientsList(data.recentPatients)
            }
            
        } else {
            console.error('Failed to load appointment statistics')
        }
    } catch (error) {
        console.error('Error loading appointment statistics:', error)
    }
}

function loadRecentPatients() {
    // This will be populated by the loadPatientStatistics function
    // For now, show a loading message
    const container = document.getElementById("recentPatients")
    container.innerHTML = '<div class="text-center text-muted">Loading recent patients...</div>'
}

// Update the loadRecentPatients function to show actual data
async function updateRecentPatientsList(patients) {
    const container = document.getElementById("recentPatients")
    
    if (!patients || patients.length === 0) {
        container.innerHTML = '<div class="text-center text-muted">No recent patient registrations</div>'
        return
    }
    
    container.innerHTML = patients.map(patient => `
        <div class="activity-item">
            <div class="activity-icon">
                <i class="fas fa-user-plus"></i>
            </div>
            <div class="activity-content">
                <div class="activity-text">
                    <strong>${patient.name}</strong> registered
                </div>
                <div class="activity-meta">
                    <span class="text-muted">${patient.email}</span>
                    <span class="badge bg-${patient.status === 'Active' ? 'success' : 'secondary'}">${patient.status}</span>
                </div>
                <small class="text-muted">${patient.registeredAt}</small>
            </div>
        </div>
    `).join('')
}

function loadSystemAlerts() {
    const alerts = [
        {
            type: "warning",
            message: "Server storage is 85% full",
            time: "5 minutes ago",
            icon: "fa-exclamation-triangle",
        },
        {
            type: "info",
            message: "System backup completed successfully",
            time: "1 hour ago",
            icon: "fa-info-circle",
        },
        {
            type: "success",
            message: "Database optimization completed",
            time: "2 hours ago",
            icon: "fa-check-circle",
        },
        {
            type: "error",
            message: "Failed login attempts detected",
            time: "3 hours ago",
            icon: "fa-times-circle",
        },
    ]

    const container = document.getElementById("systemAlerts")
    container.innerHTML = alerts
        .map(
            (alert) => `
        <div class="alert-item ${alert.type}">
            <div class="alert-icon">
                <i class="fas ${alert.icon}"></i>
            </div>
            <div class="alert-content">
                <div class="alert-message">${alert.message}</div>
                <div class="alert-time">${alert.time}</div>
            </div>
        </div>
    `,
        )
        .join("")
}

function updateStatistics() {
    // Simulate real-time updates
    setInterval(() => {
        const totalUsers = document.getElementById("totalUsers")
        const currentUsers = Number.parseInt(totalUsers.textContent.replace(",", ""))
        totalUsers.textContent = (currentUsers + Math.floor(Math.random() * 3)).toLocaleString()

        const totalAppointments = document.getElementById("totalAppointments")
        const currentAppointments = Number.parseInt(totalAppointments.textContent.replace(",", ""))
        totalAppointments.textContent = (currentAppointments + Math.floor(Math.random() * 5)).toLocaleString()
    }, 30000) // Update every 30 seconds
}

function logout() {
    if (confirm("Are you sure you want to logout?")) {
        localStorage.clear()
        window.location.href = "index.html"
    }
}
