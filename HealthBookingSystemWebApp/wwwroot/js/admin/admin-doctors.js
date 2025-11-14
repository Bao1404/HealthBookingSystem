// Admin Users Management functionality
let users = []
let filteredUsers = []
let currentPage = 1
const usersPerPage = 10
const selectedUsers = new Set()
const bootstrap = window.bootstrap // Declare the bootstrap variable

document.addEventListener("DOMContentLoaded", () => {
    loadUsers()
    setupSearch()
})

async function loadUsers() {
    try {
        const params = new URLSearchParams({
            page: currentPage,
            pageSize: usersPerPage,
            role: 'doctor'
        });
        const response = await fetch('/Admin/GetUsers?' + params)
        if (response.ok) {
            const data = await response.json()
            users = data.users
            filteredUsers = [...users]
            renderUsers()
            renderPagination(data.totalPages, data.totalCount)
        } else {
            console.error('Failed to load users')
        }
    } catch (error) {
        console.error('Error loading users:', error)
    }
}

function setupSearch() {
    const searchInput = document.getElementById("searchInput")
    searchInput.addEventListener("input", debounce(async (e) => {
        const searchTerm = e.target.value
        await applyFilters()
    }, 300))
}

async function applyFilters() {
    const searchTerm = document.getElementById("searchInput").value
    const statusFilter = document.getElementById("statusFilter").value
    const fromDate = document.getElementById("fromDateFilter").value
    const toDate = document.getElementById("toDateFilter").value

    const params = new URLSearchParams({
        page: currentPage,
        pageSize: usersPerPage,
        role: 'doctor'
    })

    if (searchTerm) params.append('searchTerm', searchTerm)
    if (statusFilter) {
        const isActive = statusFilter === 'active'
        params.append('isActive', isActive)
    }
    if (fromDate) params.append('fromDate', fromDate)
    if (toDate) params.append('toDate', toDate)

    try {
        const response = await fetch('/Admin/GetUsers?' + params)
        if (response.ok) {
            const data = await response.json()
            users = data.users
            filteredUsers = [...users]
            renderUsers()
            renderPagination(data.totalPages, data.totalCount)
        } else {
            console.error('Filter request failed:', response.status)
        }
    } catch (error) {
        console.error('Error applying filters:', error)
    }
}

function clearFilters() {
    document.getElementById("statusFilter").value = ""
    document.getElementById("searchInput").value = ""
    document.getElementById("fromDateFilter").value = ""
    document.getElementById("toDateFilter").value = ""
    currentPage = 1
    loadUsers()
}

function renderUsers() {
    const tbody = document.getElementById("usersTableBody")
    tbody.innerHTML = users
        .map(
            (user) => `
        <tr>
            <td>
                <input type="checkbox" class="user-checkbox" value="${user.userId}" onchange="toggleUserSelection(${user.userId})">
            </td>
            <td>
                <div class="d-flex align-items-center">
                    <img src="${user.avatarUrl || '/placeholder.svg?height=40&width=40'}" alt="${user.fullName}" class="rounded-circle me-2" width="40" height="40">
                    <div>
                        <div class="fw-bold">${user.fullName}</div>
                        <small class="text-muted">${user.email}</small>
                    </div>
                </div>
            </td>
            <td>
                <span class="badge bg-${getRoleBadgeColor(user.role)}">${user.role.charAt(0).toUpperCase() + user.role.slice(1)}</span>
            </td>
            <td>
                <span class="badge bg-${getStatusBadgeColor(user.isActive)}">${user.isActive ? 'Active' : 'Inactive'}</span>
            </td>
            <td>${formatDate(user.createdAt)}</td>
            <td>${user.lastLogin}</td>
            <td>
                <div class="btn-group" role="group">
                    <a href="/Admin/DoctorEdit/${user.userId}" class="btn btn-sm btn-outline-primary" title="Edit">
                        <i class="fas fa-edit"></i>
                    </a>
                    <a href="/Admin/DoctorDetail/${user.userId}" class="btn btn-sm btn-outline-info" title="View">
                        <i class="fas fa-eye"></i>
                    </a>
                    <a href="/Admin/DoctorDelete/${user.userId}" class="btn btn-sm btn-outline-danger" title="Delete">
                        <i class="fas fa-trash"></i>
                    </a>
                </div>
            </td>
        </tr>
    `,
        )
        .join("")
}

function renderPagination(totalPages, totalCount) {
    const pagination = document.getElementById("pagination")
    let paginationHTML = ""

    // Previous button
    paginationHTML += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changePage(${currentPage - 1})">Previous</a>
        </li>
    `

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
            paginationHTML += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
                </li>
            `
        } else if (i === currentPage - 3 || i === currentPage + 3) {
            paginationHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`
        }
    }

    // Next button
    paginationHTML += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changePage(${currentPage + 1})">Next</a>
        </li>
    `

    pagination.innerHTML = paginationHTML
}

async function changePage(page) {
    currentPage = page
    await applyFilters()
}

function getRoleBadgeColor(role) {
    switch (role.toLowerCase()) {
        case "doctor":
            return "primary"
        default:
            return "secondary"
    }
}

function getStatusBadgeColor(isActive) {
    return isActive ? "success" : "secondary"
}

function formatDate(dateString) {
    if (!dateString) return "N/A"
    const date = new Date(dateString)
    return date.toLocaleDateString()
}

function toggleSelectAll() {
    const selectAllCheckbox = document.getElementById("selectAll")
    const userCheckboxes = document.querySelectorAll(".user-checkbox")

    userCheckboxes.forEach((checkbox) => {
        checkbox.checked = selectAllCheckbox.checked
        if (selectAllCheckbox.checked) {
            selectedUsers.add(parseInt(checkbox.value))
        } else {
            selectedUsers.delete(parseInt(checkbox.value))
        }
    })

    updateBulkActions()
}

function toggleUserSelection(userId) {
    if (selectedUsers.has(userId)) {
        selectedUsers.delete(userId)
    } else {
        selectedUsers.add(userId)
    }

    updateBulkActions()
}

function updateBulkActions() {
    const bulkActionsCard = document.getElementById("bulkActionsCard")
    const selectedCount = document.getElementById("selectedCount")
    const selectAllCheckbox = document.getElementById("selectAll")

    if (selectedUsers.size > 0) {
        bulkActionsCard.style.display = "block"
        selectedCount.textContent = selectedUsers.size

        // Update select all checkbox
        const userCheckboxes = document.querySelectorAll(".user-checkbox")
        const checkedCount = Array.from(userCheckboxes).filter(cb => cb.checked).length
        selectAllCheckbox.checked = checkedCount === userCheckboxes.length
        selectAllCheckbox.indeterminate = checkedCount > 0 && checkedCount < userCheckboxes.length
    } else {
        bulkActionsCard.style.display = "none"
        selectAllCheckbox.checked = false
        selectAllCheckbox.indeterminate = false
    }
}

async function addUser() {
    const form = document.getElementById("addUserForm")
    const formData = new FormData(form)

    const userData = {
        fullName: document.getElementById("firstName").value + " " + document.getElementById("lastName").value,
        email: document.getElementById("email").value,
        phoneNumber: document.getElementById("phone").value,
        role: document.getElementById("role").value,
        password: document.getElementById("password").value,
        isActive: document.getElementById("status").value === "active",
        sendWelcomeEmail: document.getElementById("sendWelcomeEmail").checked
    }

    try {
        const response = await fetch('/Admin/CreateUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData)
        })

        if (response.ok) {
            const result = await response.json()
            showAlert('success', result.message)
            bootstrap.Modal.getInstance(document.getElementById('addUserModal')).hide()
            form.reset()
            loadUsers()
        } else {
            const error = await response.json()
            showAlert('danger', error.error || 'Failed to create user')
        }
    } catch (error) {
        console.error('Error creating user:', error)
        showAlert('danger', 'An error occurred while creating the user')
    }
}



async function bulkAction(action) {
    if (selectedUsers.size === 0) {
        showAlert('warning', 'Please select users to perform bulk action')
        return
    }

    if (!confirm(`Are you sure you want to ${action} ${selectedUsers.size} user(s)?`)) return

    try {
        const response = await fetch('/Admin/BulkAction', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                userIds: Array.from(selectedUsers),
                action: action
            })
        })

        if (response.ok) {
            const result = await response.json()
            showAlert('success', result.message)
            selectedUsers.clear()
            updateBulkActions()
            loadUsers()
        } else {
            const error = await response.json()
            showAlert('danger', error.error || `Failed to perform bulk ${action}`)
        }
    } catch (error) {
        console.error('Error performing bulk action:', error)
        showAlert('danger', `An error occurred while performing bulk ${action}`)
    }
}

function exportUsers(format) {
    if (format === 'csv') {
        exportToCSV()
    } else if (format === 'pdf') {
        exportToPDF()
    }
}

function exportToCSV() {
    const headers = ['Name', 'Email', 'Role', 'Status', 'Registration Date', 'Last Login']
    const csvContent = [
        headers.join(','),
        ...users.map(user => [
            user.fullName,
            user.email,
            user.role,
            user.isActive ? 'Active' : 'Inactive',
            formatDate(user.createdAt),
            user.lastLogin
        ].join(','))
    ].join('\n')

    const blob = new Blob([csvContent], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'users.csv'
    a.click()
    window.URL.revokeObjectURL(url)
}

function exportToPDF() {
    // Implement PDF export functionality
    alert('PDF export functionality would be implemented here')
}

function showAlert(type, message) {
    const alertDiv = document.createElement('div')
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `

    const container = document.querySelector('.main-content')
    container.insertBefore(alertDiv, container.firstChild)

    setTimeout(() => {
        alertDiv.remove()
    }, 5000)
}

function debounce(func, wait) {
    let timeout
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout)
            func(...args)
        }
        clearTimeout(timeout)
        timeout = setTimeout(later, wait)
    }
}

// Debug function to check users in database
async function debugUsers() {
    try {
        const response = await fetch('/Admin/DebugUsers')
        if (response.ok) {
            const data = await response.json()
            console.log('Debug users data:', data)
            alert(`Total users: ${data.totalUsers}\nRoles: ${data.roles.join(', ')}`)
        }
    } catch (error) {
        console.error('Error debugging users:', error)
    }
}
