namespace HealthBookingSystem.Mapper
{
    public class SpecialtyMapper
    {
        public static string GetSpecialty(string userMessage)
        {
            var disease = userMessage.ToLower();

            // Từ khóa cho các chuyên khoa
            var specialtyKeywords = new Dictionary<string, List<string>>()
        {
            { "Tim mạch", new List<string> { "tim", "huyết áp", "tăng huyết áp", "mạch máu", "nhồi máu cơ tim", "cơn đau ngực" } },
            { "Da liễu", new List<string> { "da", "mụn", "nám", "chàm", "eczema", "viêm da", "bệnh vảy nến", "bỏng" } },
            { "Nội khoa", new List<string> { "tiểu đường", "huyết áp", "bệnh tim mạch", "hô hấp", "thận", "viêm gan", "xơ gan" } },
            { "Chấn thương chỉnh hình", new List<string> { "xương", "khớp", "cổ tay", "gãy xương", "thoái hóa khớp", "viêm khớp", "bệnh gút" } },
            { "Thần kinh", new List<string> { "đau đầu", "tai biến", "đột quỵ", "chóng mặt", "suy giảm trí nhớ", "bệnh Parkinson", "động kinh" } },
            { "Nhi khoa", new List<string> { "trẻ em", "sốt", "ho", "hen suyễn", "viêm họng", "bệnh sởi", "bạch hầu", "bệnh tay chân miệng" } },
            { "Tâm thần học", new List<string> { "trầm cảm", "lo âu", "stress", "rối loạn lo âu", "hưng phấn", "rối loạn tâm thần", "tâm lý" } },
            { "Mắt", new List<string> { "mắt", "cận thị", "loạn thị", "đau mắt", "nhìn mờ", "tật khúc xạ", "thoái hóa điểm vàng" } },
            { "Tiêu hóa", new List<string> { "dạ dày", "ruột", "tiêu hóa", "ợ nóng", "trào ngược dạ dày thực quản", "viêm đại tràng", "bệnh gan" } },
            { "Nội tiết", new List<string> { "tuyến giáp", "hormon", "đái tháo đường", "cường giáp", "suy giáp", "hội chứng buồng trứng đa nang", "hạ đường huyết" } },
            { "Tiết niệu", new List<string> { "tiết niệu", "sỏi thận", "đái dầm", "rối loạn tiểu tiện", "bàng quang", "suy thận" } },
            { "Ung thư", new List<string> { "ung thư", "khối u", "mầm bệnh", "di căn", "ung thư vú", "ung thư phổi", "ung thư đại trực tràng" } },
            // Các chuyên khoa khác có thể thêm vào đây
        };

            // Duyệt qua các chuyên khoa và kiểm tra xem có từ khóa nào khớp không
            foreach (var specialty in specialtyKeywords)
            {
                foreach (var keyword in specialty.Value)
                {
                    if (disease.Contains(keyword))
                    {
                        return specialty.Key;
                    }
                }
            }

            // Nếu không tìm thấy từ khóa nào, trả về "Chăm sóc sức khỏe tổng quát"
            return "Chăm sóc sức khỏe tổng quát";
        }
    }
}
