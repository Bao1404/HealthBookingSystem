// AI Chat functionality
let isTyping = false;

document.addEventListener("DOMContentLoaded", () => {
    const userId = document.getElementById("userId").innerText;
    if (userId) getMessages(userId);

    // Ẩn khung bác sĩ ban đầu
    document.getElementById("recommendedDoctorsCard").style.display = "none";
});

// Gửi tin nhắn lên API
function sendAIMessage(e) {
    e.preventDefault();
    const input = document.getElementById("aiMessageInput");
    const text = input.value.trim();
    if (!text || isTyping) return;

    addMessage("user", text);
    input.value = "";
    showTypingIndicator();

    const userId = parseInt(document.getElementById("userId").innerText, 10);
    fetch("/api/chatbox/message", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ UserId: userId, Message: text })
    })
        .then(r => r.json())
        .then(data => {
            hideTypingIndicator();
            addMessage("ai", data.aiReply);

            // Nếu có danh sách bác sĩ gợi ý
            if (data.recommendedDoctors?.length) {
                renderRecommendedDoctors(data.recommendedDoctors);
            } else {
                document.getElementById("recommendedDoctorsCard").style.display = "none";
            }
        })
        .catch(err => {
            hideTypingIndicator();
            console.error(err);
            addMessage("ai", "Xin lỗi, có lỗi. Vui lòng thử lại sau.");
        });
}

// Render khung bác sĩ
function renderRecommendedDoctors(doctors) {
    const card = document.getElementById("recommendedDoctorsCard");
    const grid = document.getElementById("recommendedDoctors");
    grid.innerHTML = doctors.map(d => `
    <div class="recommended-doctor-card">
      <div class="doctor-card-header">
        <img src="${d.avatar}"
             alt="${d.fullName}" class="doctor-card-avatar">
        <div class="doctor-card-info">
          <h6>${d.fullName}</h6>
          <p>${d.specialty}</p>
          <span class="doctor-experience">${d.experience}</span>
        </div>
      </div>
      <div class="doctor-card-footer">
        <a href="/User/Appointments?doctorId=${d.userId}&specialtyId=${d.specialtyId}" class="btn btn-sm btn-primary">
            Đặt lịch
        </a>
      </div>
    </div>
  `).join("");
    card.style.display = "block";
}

// Thêm tin nhắn vào giao diện
function addMessage(sender, text) {
    const messagesContainer = document.getElementById("aiChatMessages");
    const time = new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });

    const messageElement = document.createElement("div");
    messageElement.className = `message ${sender === "user" ? "user-message" : "ai-message"}`;

    // Format tin nhắn trước khi hiển thị
    const formattedText = formatMessageText(text);

    messageElement.innerHTML = `
    <div class="message-avatar">
      ${sender === "user"
            ? '<img src="/placeholder.svg?height=36&width=36" alt="You">'
            : '<div class="ai-avatar-small"><i class="fas fa-robot"></i></div>'}
    </div>
    <div class="message-content">
      <div class="message-header">
        <span class="message-sender">${sender === "user" ? "You" : "AI Assistant"}</span>
        <span class="message-time">${time}</span>
      </div>
      <div class="message-text">${formattedText}</div>
      ${sender === "ai" ? `
        <div class="ai-disclaimer">
          <i class="fas fa-exclamation-triangle"></i>
          Đây là thông tin tổng quát, không thay thế ý kiến chuyên môn.
        </div>
      ` : ""}
    </div>
  `;
    messagesContainer.appendChild(messageElement);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

// Hàm định dạng tin nhắn và xóa mọi dấu *
function formatMessageText(responseText) {
    if (!responseText) return "";
    // 1. Xóa tất cả dấu *
    const clean = responseText.replace(/\*/g, "");
    // 2. Chuyển mỗi dòng thành <p>
    return clean
        .split("\n")
        .map(line => line.trim())
        .filter(line => line.length > 0)
        .map(line => `<p>${line}</p>`)
        .join("");
}

// Hiển thị biểu tượng đang gõ
function showTypingIndicator() {
    if (isTyping) return;
    isTyping = true;
    const container = document.getElementById("aiChatMessages");
    const el = document.createElement("div");
    el.className = "ai-typing-indicator";
    el.id = "typingIndicator";
    el.innerHTML = `
    <div class="message-avatar">
      <div class="ai-avatar-small"><i class="fas fa-robot"></i></div>
    </div>
    <div class="typing-content">
      <span class="typing-text">AI đang gõ…</span>
      <div class="typing-dots"><span></span><span></span><span></span></div>
    </div>`;
    container.appendChild(el);
    container.scrollTop = container.scrollHeight;
}

// Ẩn biểu tượng đang gõ
function hideTypingIndicator() {
    isTyping = false;
    const el = document.getElementById("typingIndicator");
    if (el) el.remove();
}

// Lấy lịch sử tin nhắn từ API
function getMessages(userId) {
    fetch(`/api/chatbox/messages/${userId}`)
        .then(res => res.json())
        .then(data => {
            data.forEach(msg => {
                addMessage(msg.sender.toLowerCase(), msg.content);
            });
        })
        .catch(err => console.error("Error fetching messages:", err));
}


// Đặt lịch với bác sĩ
function bookWithDoctor(doctorId) {
    localStorage.setItem("selectedDoctorId", doctorId);
    window.location.href = "/appointments"; // hoặc route tương ứng
}
