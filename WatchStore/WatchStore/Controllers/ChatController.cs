using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WatchStore.Models;
using WatchStore.Services;

namespace WatchStore.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatGPTService _chatService;
        private readonly QuestionAnalysisService _questionAnalysis;
        private readonly WatchStoreDbContext _db;

        public ChatController()
        {
            _chatService = new ChatGPTService();
            _questionAnalysis = new QuestionAnalysisService();
            _db = new WatchStoreDbContext();
        }

        [HttpPost]
        public async Task<ActionResult> Ask(string userMessage)
        {
            var analysis = _questionAnalysis.AnalyzeQuestion(userMessage);

            if (analysis.Intent == "product_query")
            {
                var query = _db.Products.Where(p => p.Status == 1);

                // Áp dụng các bộ lọc dựa trên phân tích
                if (analysis.Brand != null)
                {
                    query = query.Where(p => p.Name.ToLower().Contains(analysis.Brand));
                }

                if (analysis.PriceMin.HasValue)
                {
                    query = query.Where(p => p.Price >= analysis.PriceMin.Value);
                }

                if (analysis.PriceMax.HasValue)
                {
                    query = query.Where(p => p.Price <= analysis.PriceMax.Value);
                }

                if (analysis.Type != null)
                {
                    query = query.Where(p => p.MetaDesc.ToLower().Contains(analysis.Type));
                }

                if (analysis.Gender != null)
                {
                    query = query.Where(p => p.MetaDesc.ToLower().Contains(analysis.Gender.ToLower()));
                }

                // Lấy sản phẩm phù hợp
                var products = await query
                    .OrderByDescending(p => p.Created_at)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Name,
                        p.Price,
                        p.MetaDesc,
                        p.Image,
                        p.Detail
                    })
                    .ToListAsync();

                if (products.Any())
                {
                    string response;
                    if (analysis.QueryType == "compare" && products.Count >= 2)
                    {
                        response = "Đây là so sánh giữa các sản phẩm:\n\n";
                        foreach (var product in products)
                        {
                            response += $"- {product.Name}:\n";
                            response += $"  + Giá: {product.Price:N0} VND\n";
                            response += $"  + Đặc điểm: {product.MetaDesc}\n";
                            response += $"  + Chi tiết: {product.Detail}\n\n";
                        }
                    }
                    else if (analysis.HasPriceQuery)
                    {
                        response = "Thông tin giá các sản phẩm phù hợp:\n\n";
                        foreach (var product in products)
                        {
                            response += $"- {product.Name}: {product.Price:N0} VND\n";
                            response += $"  {product.MetaDesc}\n\n";
                        }
                    }
                    else
                    {
                        response = "Đây là một số sản phẩm phù hợp với yêu cầu của bạn:\n\n";
                        foreach (var product in products)
                        {
                            response += $"- {product.Name}\n";
                            response += $"  + Giá: {product.Price:N0} VND\n";
                            response += $"  + Mô tả: {product.MetaDesc}\n\n";
                        }
                    }

                    return Json(new { response = response });
                }
                else
                {
                    // Nếu không tìm thấy sản phẩm phù hợp, sử dụng ChatGPT để gợi ý
                    var reply = await _chatService.AskChatGPT(
                        $"Tôi không tìm thấy sản phẩm phù hợp với yêu cầu: {userMessage}. " +
                        "Hãy gợi ý cho khách hàng một số tiêu chí khác hoặc đề xuất thay đổi yêu cầu.");
                    return Json(new { response = reply });
                }
            }
            else
            {
                // Đối với câu hỏi chung, sử dụng ChatGPT
                var reply = await _chatService.AskChatGPT(userMessage);
                return Json(new { response = reply });
            }
        }
    }
}
