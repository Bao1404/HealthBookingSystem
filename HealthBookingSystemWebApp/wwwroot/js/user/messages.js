let currentConversation = null;
let conversations = [];
let connection = null;

document.addEventListener("DOMContentLoaded", () => {
    const userId = document.getElementById("PatientId").innerText;
    console.log("Loaded patientId:", userId);
        
    loadConversations();
    setupEventListeners();
});

async function setupSignalR(conversationId) {
    currentConversation = conversations.find((c) => c.conversationId === conversationId)

    conversationId = String(conversationId);
    if (!conversationId || typeof conversationId !== 'string' || conversationId.trim() === '') {
        console.error("conversationId không hợp lệ:", conversationId);
        showNotification("Không thể kết nối: ID cuộc trò chuyện không hợp lệ", "danger");
        return;
    }

    conversationId = conversationId.trim();

            var senderIdPatient = document.getElementById("PatientId").innerText;
    var patientName = currentConversation.patientUser?.fullName;
    if (!connection) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`/chathub?conversationId=${encodeURIComponent(conversationId)}`)
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveMessage", (senderId, message) => {
            console.log("📨 Tin nhắn mới nhận được:", senderId, "Tin nhắn:", message);
            loadMessagesFromApi(conversationId);
        });

        connection.on("ReceiveCall", (senderId, message) => {
            console.log("📞 Cuộc gọi đến từ:", senderId, "Tin nhắn:", message, "ConversationId:", conversationId);

            // Lấy doctorName từ conversations
            const conversation = conversations.find(c => String(c.conversationId) === conversationId);
            const doctorName = conversation?.doctorUser?.fullName || "Bác sĩ không xác định";

            // Hiển thị thông báo
            showNotification(`Cuộc gọi đến từ ${senderId} (${doctorName}): ${message}`, "info");

            // Hỏi người dùng có muốn tham gia không
            if (confirm(`Bạn có cuộc gọi đến từ ${senderId} (${doctorName}). Bạn muốn tham gia không?`)) {
                // Lưu các biến vào localStorage
                localStorage.setItem("conversationId", conversationId);
                localStorage.setItem("senderId", senderIdPatient);
                localStorage.setItem("doctorName", doctorName);

                // Chuyển hướng đến trang Call.html
                window.location.href = `/static/Call2.html?conversationId=${conversationId}&patientId=${senderIdPatient}&patientName=${patientName}`;
            }
        });

        connection.onclose((error) => {
            console.error("Kết nối SignalR bị đóng:", error);
            showNotification("Kết nối SignalR bị đóng bất ngờ", "danger");
        });

        try {
            console.log("Bắt đầu kết nối SignalR...");
            await connection.start();
            console.log("🟢 Đã kết nối SignalR, trạng thái:", connection.state);
        } catch (err) {
            console.error("Lỗi kết nối SignalR:", err);
            showNotification("Kết nối SignalR thất bại: " + err.message, "danger");
            return;
        }
    }

    if (connection.state === signalR.HubConnectionState.Connected) {
        await connection.invoke("JoinGroup", conversationId);
        console.log(`Đã tham gia nhóm cho conversationId: ${conversationId}`);
    } else {
        console.error("Kết nối SignalR không ở trạng thái Connected:", connection.state);

    }
}
function showNotification(message, type = "info") {
    const notification = document.createElement("div");
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = "top: 20px; right: 20px; z-index: 9999; min-width: 300px;";
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(notification);

    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 10000);
}
//function updateUserInfo() {
//    const userName = localStorage.getItem("userName")
//    document.getElementById("userName").textContent = userName
//}

async function loadConversations() {
    const userId = document.getElementById("PatientId").innerText;
    console.log("Loaded patientId:", userId)
    if (!userId) return

    try {
        const response = await fetch(`/api/ApiConversation/patient/${userId}`)
        if (!response.ok) throw new Error("Failed to load conversations")

        const data = await response.json()
        console.log("✅ Conversations data:", data) // ✅ In ra để kiểm tra

        conversations = data
        renderConversations()
    } catch (err) {
        console.error("❌ Error loading conversations:", err)
    }
}

function renderConversations() {
    const container = document.getElementById("conversationsList")
    container.innerHTML = conversations.map((conversation) => `
        <div class="conversation-item" onclick="selectConversation(${conversation.conversationId})">
            <div class="conversation-avatar">
                <img src="${conversation.doctorUser?.avatarUrl || '/placeholder.svg?height=48&width=48'}" alt="${conversation.doctorUser?.fullName || 'Doctor'}">
                <div class="status-indicator online"></div>
            </div>
            <div class="conversation-content">
                <div class="conversation-header">
                    <h6 class="conversation-name">${conversation.doctorUser?.fullName || 'Doctor'}</h6>
                    <span class="conversation-time">${new Date(conversation.updatedAt).toLocaleTimeString()}</span>
                </div>
                <p class="conversation-specialty">ID: ${conversation.doctorUser?.userId}</p>
                <p class="conversation-preview">Click to view messages</p>
            </div>
        </div>
    `).join("")
}

function selectConversation(conversationId) {
    currentConversation = conversations.find((c) => c.conversationId === conversationId)
    if (!currentConversation) return

    document.querySelectorAll(".conversation-item").forEach((item) => item.classList.remove("active"))
    event.currentTarget.classList.add("active")

    showChatInterface()
    loadMessagesFromApi(conversationId)
    setupSignalR(conversationId);
    console.log("📬 Selected conversation:", currentConversation)
}

async function loadMessagesFromApi(conversationId) {
    const container = document.getElementById("chatMessages")
    container.innerHTML = "<p>Loading...</p>"

    try {
        setupSignalR(conversationId); console.log("🔄 Loading messages for conversation:", conversationId)
        const res = await fetch(`/api/APIMessage/conversation/${conversationId}`)
        if (!res.ok) throw new Error("Failed to load messages")

        const messages = await res.json()
        container.innerHTML = messages.map((message) => {
            const sender = message.sender || {}
            const senderRole = sender.role || "Unknown"
            const senderName = sender.fullName || "Unknown"
            const senderAvatar = sender.avatarUrl || "/placeholder.svg?height=36&width=36"

            return `
                <div class="message ${senderRole === "Patient" ? "user-message" : "doctor-message"}">
                    <div class="message-avatar">
                        <img src="${senderAvatar}" alt="${senderName}">
                    </div>
                    <div class="message-content">
                        <div class="message-header">
                            <span class="message-sender">${senderRole === "Patient" ? "You" : senderName}</span>
                            <span class="message-time">${new Date(message.sentAt).toLocaleTimeString()}</span>
                        </div>
                        <div class="message-text">${message.content}</div>
                    </div>
                </div>
            `
        }).join("")

        container.scrollTop = container.scrollHeight
    } catch (err) {
        console.error("❌ Error loading messages:", err)

        container.innerHTML = "<p>Failed to load messages.</p>"
    }
}

function showChatInterface() {
    if (!currentConversation) return

    document.getElementById("chatHeader").style.display = "flex"
    document.getElementById("chatInputContainer").style.display = "block"
    document.getElementById("chatAvatar").src = currentConversation.doctorUser?.avatarUrl
    document.getElementById("chatDoctorName").textContent = currentConversation.doctorUser?.fullName
    document.getElementById("chatDoctorSpecialty").textContent = "Doctor"

    const emptyChat = document.querySelector(".empty-chat")
    if (emptyChat) emptyChat.style.display = "none"
}



async function sendMessage(event) {
    event.preventDefault();

    if (!currentConversation) return;

    const input = document.getElementById("messageInput");
    const messageText = input.value.trim();
    if (!messageText) return;

    const conversationId = currentConversation.conversationId;
    const senderId = parseInt(document.getElementById("PatientId").innerText); // 👈 role: bệnh nhân
    const receiverId = currentConversation.doctorUser?.userId;

    console.log("👤 Sender (patient):", senderId);
    console.log("📥 Receiver (doctor):", receiverId);
    console.log("🧵 Conversation:", conversationId);

    if (!senderId || !receiverId || !conversationId) {
        console.error("❌ Missing senderId, receiverId, or conversationId");
        return;
    }

    await setupSignalR(conversationId); // Khởi tạo kết nối SignalR nếu cần

    const newMessage = {
        conversationId: conversationId,
        senderId: senderId,
        messageType: "text",
        content: messageText
    };

    try {
        const res = await fetch("/api/APIMessage/send", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(newMessage)
        });

        if (!res.ok) throw new Error("Failed to send message");

        // Gửi socket thông báo tới bên còn lại
        await connection.invoke("SendMessage", conversationId.toString(), senderId.toString(), messageText);

        // Xóa input & cập nhật UI
        input.value = "";
        await loadMessagesFromApi(conversationId);

    } catch (err) {
        console.error("❌ Error sending message:", err);
    }
}


function logout() {
    if (confirm("Are you sure you want to logout?")) {
        localStorage.clear()
        window.location.href = "index.html"
    }
}
