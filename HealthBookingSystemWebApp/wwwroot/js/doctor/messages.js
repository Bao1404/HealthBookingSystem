// Doctor Messages functionality
let currentConversation = null
let conversations = []

document.addEventListener("DOMContentLoaded", () => {
    initializeMessages()
    loadConversations()

    setupEventListeners()
})

function initializeMessages() {
    const doctorName = localStorage.getItem("doctorName") || "Dr. Sarah Johnson"
    document.getElementById("doctorName").textContent = doctorName

    // Initialize conversations data
    conversations = [
        {
            id: 1,
            patientId: 1,
            patientName: "John Doe",
            patientInfo: "Age 45 • Hypertension",
            avatar: "/placeholder.svg?height=48&width=48",
            lastMessage: "Thank you for the prescription, Doctor.",
            lastMessageTime: "10:30 AM",
            unreadCount: 2,
            status: "online",
            messages: [
                {
                    id: 1,
                    sender: "patient",
                    text: "Hello Doctor, I have been experiencing some chest pain.",
                    time: "09:15 AM",
                    timestamp: new Date("2024-01-20T09:15:00"),
                },
                {
                    id: 2,
                    sender: "doctor",
                    text: "I understand your concern. Can you describe the pain? Is it sharp or dull?",
                    time: "09:18 AM",
                    timestamp: new Date("2024-01-20T09:18:00"),
                },
                {
                    id: 3,
                    sender: "patient",
                    text: "It's more of a dull ache, and it comes and goes.",
                    time: "09:20 AM",
                    timestamp: new Date("2024-01-20T09:20:00"),
                },
                {
                    id: 4,
                    sender: "doctor",
                    text: "Based on your symptoms and medical history, I'm prescribing a medication adjustment. Please schedule a follow-up appointment.",
                    time: "09:25 AM",
                    timestamp: new Date("2024-01-20T09:25:00"),
                },
                {
                    id: 5,
                    sender: "patient",
                    text: "Thank you for the prescription, Doctor.",
                    time: "10:30 AM",
                    timestamp: new Date("2024-01-20T10:30:00"),
                },
            ],
        },
        {
            id: 2,
            patientId: 2,
            patientName: "Jane Smith",
            patientInfo: "Age 32 • Diabetes",
            avatar: "/placeholder.svg?height=48&width=48",
            lastMessage: "My blood sugar levels have been stable.",
            lastMessageTime: "Yesterday",
            unreadCount: 0,
            status: "away",
            messages: [
                {
                    id: 1,
                    sender: "patient",
                    text: "Good morning Doctor! I wanted to update you on my blood sugar readings.",
                    time: "Yesterday 2:00 PM",
                    timestamp: new Date("2024-01-19T14:00:00"),
                },
                {
                    id: 2,
                    sender: "doctor",
                    text: "Good to hear from you, Jane. How have the readings been?",
                    time: "Yesterday 2:05 PM",
                    timestamp: new Date("2024-01-19T14:05:00"),
                },
                {
                    id: 3,
                    sender: "patient",
                    text: "My blood sugar levels have been stable.",
                    time: "Yesterday 2:10 PM",
                    timestamp: new Date("2024-01-19T14:10:00"),
                },
            ],
        },
        {
            id: 3,
            patientId: 3,
            patientName: "Mike Johnson",
            patientInfo: "Age 28 • Anxiety",
            avatar: "/placeholder.svg?height=48&width=48",
            lastMessage: "The breathing exercises are helping.",
            lastMessageTime: "2 days ago",
            unreadCount: 1,
            status: "offline",
            messages: [
                {
                    id: 1,
                    sender: "patient",
                    text: "Hi Dr. Johnson, I've been practicing the breathing exercises you recommended.",
                    time: "2 days ago 11:00 AM",
                    timestamp: new Date("2024-01-18T11:00:00"),
                },
                {
                    id: 2,
                    sender: "doctor",
                    text: "That's great to hear! How are you feeling?",
                    time: "2 days ago 11:15 AM",
                    timestamp: new Date("2024-01-18T11:15:00"),
                },
                {
                    id: 3,
                    sender: "patient",
                    text: "The breathing exercises are helping.",
                    time: "2 days ago 11:30 AM",
                    timestamp: new Date("2024-01-18T11:30:00"),
                },
            ],
        },
    ]
}

async function loadConversations() {
    const container = document.getElementById("conversationsList")
    if (!container) return

    const doctorId = localStorage.getItem("doctorId")

    console.log("Loaded doctorId:", doctorId); // kiểm tra giá trị

    if (!doctorId) {
        console.error("doctorId not found in localStorage");
        return;
    }

    if (!doctorId) {
        container.innerHTML = "<p>No doctor ID found.</p>"
        return
    }

    try {
        const response = await fetch(`/api/ApiConversation/doctor/${doctorId}`)
        if (!response.ok) throw new Error("Failed to fetch")

        const data = await response.json()
        conversations = data

        container.innerHTML = conversations
            .map((conversation) => `
                <div class="conversation-item" onclick="selectConversation(${conversation.conversationId})">
                   <div class="conversation-avatar">
                        <img src="${conversation.patientUser?.avatarUrl || '/placeholder.svg?height=48&width=48'}" alt="${conversation.patientUser?.fullName || 'Patient'}">
                        <div class="status-indicator online"></div>
                    </div>
                    <div class="conversation-content">
                        <div class="conversation-header">
                            <h6 class="conversation-name">${conversation.patientUser?.fullName || 'Patient'}</h6>
                            <span class="conversation-time">${new Date(conversation.updatedAt).toLocaleTimeString()}</span>
                        </div>
                        <p class="conversation-specialty">ID: ${conversation.patientUser?.userId}</p>
                        <p class="conversation-preview">Click to view messages</p>
                    </div>
                </div>
            `)
            .join("")
    } catch (err) {
        console.error("Error:", err)
        container.innerHTML = "<p>Failed to load conversations.</p>"
    }
}

function selectConversation(conversationId) {
    console.log("🔍 Selected conversationId:", conversationId); // In ra ID đã chọn

    // Remove active class from all conversations
    document.querySelectorAll(".conversation-item").forEach((item) => {
        item.classList.remove("active");
    });

    // Add active class to selected conversation
    const selectedConversationElement = event.currentTarget;
    selectedConversationElement.classList.add("active");

    // Find the conversation
    currentConversation = conversations.find((c) => c.conversationId === conversationId);
    if (!currentConversation) {
        console.warn("⚠️ Conversation not found");
        return;
    }

    // Mark as read
    currentConversation.unreadCount = 0;
    selectedConversationElement.classList.remove("unread");
    const badge = selectedConversationElement.querySelector(".unread-badge");
    if (badge) badge.remove();

    // Show chat interface
    showChatInterface();

    // Connect to SignalR immediately when a conversation is selected
    setupSignalR(conversationId);

    console.log("🟢 Setup SignalR for conversationId:", conversationId);
    // 🟢 Gọi API lấy message
    loadMessagesFromApi(conversationId);
    console.log(currentConversation.conversationId);

}



async function loadMessagesFromApi(conversationId) {
    const messagesContainer = document.getElementById("chatMessages");
    messagesContainer.innerHTML = "<p>Loading...</p>";

    try {
        const res = await fetch(`/api/APIMessage/conversation/${conversationId}`);
        if (!res.ok) throw new Error("Failed to load messages");

        const messages = await res.json();
        console.log("✅ Loaded messages from API:", messages);

        messagesContainer.innerHTML = messages
            .map((message) => {
                const sender = message.sender || {};
                const senderRole = sender.role || "Unknown";
                const senderName = sender.fullName || "Unknown";
                const senderAvatar = sender.avatarUrl || "/placeholder.svg?height=36&width=36"; // ✅ avatar từ API

                return `
                    <div class="message ${senderRole === "Doctor" ? "user-message" : "doctor-message"}">
                        <div class="message-avatar">
                            <img src="${senderAvatar}" alt="${senderName}">
                        </div>
                        <div class="message-content">
                            <div class="message-header">
                                <span class="message-sender">${senderRole === "Doctor" ? "You" : senderName}</span>
                                <span class="message-time">${new Date(message.sentAt).toLocaleTimeString()}</span>
                            </div>
                            <div class="message-text">${message.content}</div>
                        </div>
                    </div>
                `;
            })
            .join("");

        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    } catch (err) {
        console.error("❌ Error loading messages:", err);
        messagesContainer.innerHTML = "<p>Failed to load messages.</p>";
    }
}




function showChatInterface() {
    if (!currentConversation) return

    // Show chat header and input
    document.getElementById("chatHeader").style.display = "flex"
    document.getElementById("chatInputContainer").style.display = "block"

    // Update chat header
    document.getElementById("chatAvatar").src = currentConversation.patientUser?.avatarUrl
    document.getElementById("chatPatientName").textContent = currentConversation.patientName
    document.getElementById("chatPatientInfo").textContent = currentConversation.patientInfo
    console.log(chatAvatar)
    console.log(currentConversation.avatarU)
    // Hide empty chat message
    const emptyChat = document.querySelector(".empty-chat")
    if (emptyChat) emptyChat.style.display = "none"
}

function loadMessages() {

    if (!currentConversation) return
    setupSignalR(conversationId);
    const messagesContainer = document.getElementById("chatMessages")
    messagesContainer.innerHTML = currentConversation.messages
        .map(
            (message) => `
      <div class="message ${message.sender === "doctor" ? "user-message" : "doctor-message"}">
        <div class="message-avatar">
          <img src="${message.sender === "doctor" ? "/placeholder.svg?height=36&width=36" : currentConversation.avatar}" alt="${message.sender}">
        </div>
        <div class="message-content">
          <div class="message-header">
            <span class="message-sender">${message.sender === "doctor" ? "You" : currentConversation.chat - avatar}</span>
            <span class="message-time">${message.time}</span>
          </div>
          <div class="message-text">${message.text}</div>
        </div>
      </div>
    `,
        )
        .join("")

    // Scroll to bottom
    messagesContainer.scrollTop = messagesContainer.scrollHeight
}

function setupEventListeners() {
    // Search functionality
    const searchInput = document.getElementById("searchInput")
    if (searchInput) {
        searchInput.addEventListener("input", (e) => {
            filterConversations(e.target.value)
        })
    }

    // Message input
    const messageInput = document.getElementById("messageInput")
    if (messageInput) {
        messageInput.addEventListener("keypress", (e) => {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault()
                sendMessage(e)
            }
        })
    }
}

function filterConversations(searchTerm) {
    const items = document.querySelectorAll(".conversation-item")
    items.forEach((item) => {
        const name = item.querySelector(".conversation-name").textContent.toLowerCase()
        const preview = item.querySelector(".conversation-preview").textContent.toLowerCase()

        if (name.includes(searchTerm.toLowerCase()) || preview.includes(searchTerm.toLowerCase())) {
            item.style.display = "block"
        } else {
            item.style.display = "none"
        }
    })
}
let connection = null;

async function setupSignalR(conversationId) {
    conversationId = String(conversationId);
    if (!conversationId || typeof conversationId !== 'string' || conversationId.trim() === '') {
        console.error("conversationId không hợp lệ:", conversationId);
        showNotification("Không thể kết nối: ID cuộc trò chuyện không hợp lệ", "danger");
        return;
    }

    conversationId = conversationId.trim();
    if (connection) {
        console.log("Đã kết nối SignalR");
        return;
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chathub?conversationId=${encodeURIComponent(conversationId)}`)
        .withAutomaticReconnect() // Tự động thử lại kết nối
        .build();

    connection.on("ReceiveMessage", (senderId, message) => {
        console.log("📨 Tin nhắn mới nhận được:", senderId, "Tin nhắn:", message);
        loadMessagesFromApi(conversationId);
    });

    connection.on("ReceiveCall", (senderId, message) => {

        window.location.href = `/static/Call.html`;

    });

    connection.onclose((error) => {
        console.error("Kết nối SignalR bị đóng:", error);
        showNotification("Kết nối SignalR bị đóng bất ngờ", "danger");
    });

    try {
        console.log("Bắt đầu kết nối SignalR cho conversationId:", conversationId);
        await connection.start();
        console.log("🟢 Đã kết nối SignalR, trạng thái:", connection.state);

        if (connection.state === signalR.HubConnectionState.Connected) {
            await connection.invoke("JoinGroup", conversationId);
            console.log(`Đã tham gia nhóm cho conversationId: ${conversationId}`);
        } else {
            console.error("Kết nối SignalR không ở trạng thái Connected:", connection.state);
            showNotification("Kết nối SignalR chưa được thiết lập", "danger");
        }
    } catch (err) {
        console.error("Lỗi kết nối SignalR:", err);
        showNotification("Kết nối SignalR thất bại: " + err.message, "danger");
    }
}
async function startVoiceCall() {
    if (!currentConversation) {
        console.error("currentConversation là null hoặc undefined");
        showNotification("Không thể bắt đầu cuộc gọi: Không có cuộc trò chuyện nào", "danger");
        return;
    }

    const conversationId = String(currentConversation.conversationId); // Ép kiểu thành chuỗi
    const doctorName = currentConversation.doctorUser?.fullName || "Unknown Doctor";

    console.log("conversationId:", conversationId, "Kiểu:", typeof conversationId); // Log giá trị
    if (!conversationId || conversationId.trim() === '') {
        console.error("conversationId không hợp lệ:", conversationId);
        showNotification("Không thể bắt đầu cuộc gọi: Thiếu ID cuộc trò chuyện", "danger");
        return;
    }

    const doctorId = localStorage.getItem("doctorId");
    console.log("doctorId:", doctorId, "Kiểu:", typeof doctorId); // Log giá trị
    if (!doctorId || doctorId.trim() === '') {
        console.error("doctorId không hợp lệ trong localStorage");
        showNotification("Không thể bắt đầu cuộc gọi: Thiếu ID bác sĩ", "danger");
        return;
    }

    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        console.error("Kết nối SignalR chưa được thiết lập");
        showNotification("Không thể bắt đầu cuộc gọi: Kết nối SignalR chưa được thiết lập", "danger");
        return;
    }

    try {
        console.log("Sending SignalR StartCall message...");
        await connection.invoke("StartCall", conversationId, doctorId, "Call started from " + doctorName);
        console.log("📡 Signal sent to patients in conversationId:", conversationId);

        localStorage.setItem("conversationId", conversationId);
        localStorage.setItem("doctorName", doctorName);
        localStorage.setItem("doctorId", doctorId);

        window.location.href = `/static/Call.html`;
        showNotification("Cuộc gọi thoại đã bắt đầu", "info");
    } catch (err) {
        console.error("Failed to send SignalR message:", err);
        showNotification("Không thể bắt đầu cuộc gọi", "danger");
    }
}
async function sendMessage(event) {
    event.preventDefault();

    if (!currentConversation) return;

    const messageInput = document.getElementById("messageInput");
    const messageText = messageInput.value.trim();
    if (!messageText) return;

    const conversationId = currentConversation.conversationId;
    const senderId = parseInt(localStorage.getItem("doctorId")); // Có thể đổi sang patientId nếu là bệnh nhân
    const receiverId = currentConversation.patientUser?.userId || currentConversation.doctorUserId;

    console.log("📥 Sending from:", senderId);
    console.log("📤 Sending to:", receiverId);
    console.log("💬 Message:", messageText);

    if (!senderId || !receiverId || !conversationId) {
        console.error("❌ Missing senderId, receiverId, or conversationId");
        return;
    }

    // Bắt đầu kết nối socket nếu chưa có
    await setupSignalR(conversationId);

    const newMessage = {
        conversationId: conversationId,
        senderId: senderId,
        messageType: "text",
        content: messageText
    };

    try {
        // Gửi message qua API
        const res = await fetch("/api/APIMessage/send", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(newMessage)
        });

        if (!res.ok) throw new Error("Failed to send message");

        // Gửi qua socket sau khi lưu thành công (nếu muốn)
        await connection.invoke("SendMessage", conversationId.toString(), senderId.toString(), messageText);

        messageInput.value = "";
        await loadMessagesFromApi(conversationId);

    } catch (err) {
        console.error("❌ Error sending message:", err);
        showNotification("Failed to send message", "danger");
    }
}


// Action functions
function startVideoCall() {
    if (!currentConversation) return
    console.log("Starting video call with:", currentConversation.patientName)
    // Implement video call functionality
    showNotification("Video call started", "info")
}



function viewPatientProfile() {
    if (!currentConversation) return
    console.log("Viewing profile for:", currentConversation.patientName)
    window.location.href = `doctor-patients.html?id=${currentConversation.patientId}`
}

function scheduleAppointment() {
    if (!currentConversation) return
    console.log("Scheduling appointment for:", currentConversation.patientName)
    // Open appointment modal or navigate to scheduling
    window.location.href = `doctor-appointments.html?patient=${currentConversation.patientId}`
}

function attachFile() {
    console.log("Attaching file")
    // Implement file attachment functionality
    const input = document.createElement("input")
    input.type = "file"
    input.accept = "image/*,.pdf,.doc,.docx"
    input.onchange = (e) => {
        const file = e.target.files[0]
        if (file) {
            console.log("File selected:", file.name)
            // Handle file upload
        }
    }
    input.click()
}

function startNewConversation() {
    console.log("Starting new conversation")
    // Show patient selection modal or navigate to patients page
    window.location.href = "doctor-patients.html"
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
