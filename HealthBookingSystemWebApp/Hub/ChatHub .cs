using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }
    public async Task SendMessage(string conversationId, string senderId, string message)
    {
        await Clients.Group(conversationId).SendAsync("ReceiveMessage", senderId, message);
    }



    public async Task JoinGroup(string conversationId)
    {
        if (string.IsNullOrEmpty(conversationId))
        {
            _logger.LogError("conversationId is null or empty.");
            throw new ArgumentException("conversationId không được null hoặc rỗng");
        }

        try
        {
            _logger.LogInformation("Người dùng {ConnectionId} đang cố gắng tham gia nhóm {ConversationId}", Context.ConnectionId, conversationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation("Người dùng {ConnectionId} đã tham gia nhóm {ConversationId} thành công", Context.ConnectionId, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi trong JoinGroup cho conversationId {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task StartCall(string conversationId, string doctorId, string message)
    {
        Console.WriteLine($"StartCall được gọi cho cuộc trò chuyện {conversationId} với bác sĩ {doctorId}");

        if (string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(doctorId))
        {
            throw new ArgumentException("ID cuộc trò chuyện hoặc ID bác sĩ không hợp lệ");
        }

        try
        {
            await Clients.Group(conversationId).SendAsync("ReceiveCall", doctorId, message);
            Console.WriteLine($"Tín hiệu được gửi tới nhóm {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi trong StartCall: {ex.Message}");
            throw;
        }
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var conversationId = httpContext?.Request.Query["conversationId"].ToString();
        Console.WriteLine($"OnConnectedAsync: conversationId = {conversationId}");

        if (!string.IsNullOrEmpty(conversationId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            Console.WriteLine($"Người dùng {Context.ConnectionId} đã tham gia nhóm {conversationId} khi kết nối");
        }
        else
        {
            Console.WriteLine("Cảnh báo: Không có conversationId trong query string");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        var conversationId = httpContext?.Request.Query["conversationId"].ToString();

        if (!string.IsNullOrEmpty(conversationId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
            Console.WriteLine($"Người dùng {Context.ConnectionId} đã rời nhóm {conversationId} khi ngắt kết nối");
        }

        await base.OnDisconnectedAsync(exception);
    }
}