using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QuanLiNhanSu.Hubs
{
    public class ChamCongHub : Hub
    {
        // Hub này được sử dụng để Server chủ động đẩy thông báo
        // về trình duyệt (Client) khi có kết quả quét vân tay từ máy ZKTeco/Postman.
    }
}
