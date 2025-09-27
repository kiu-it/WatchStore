using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WatchStore.Models
{
    public class ChatAnalysis
    {
        public string Intent { get; set; }   // "product_query", "product_info", "general_question"
        public string QueryType { get; set; } // "search", "compare", "recommend", "info"
        
        // Thông tin sản phẩm
        public string Brand { get; set; }    // Orient, Tissot, Citizen...
        public double? PriceMin { get; set; }
        public double? PriceMax { get; set; }
        public string Gender { get; set; } // Nam, Nữ, Unisex
        public string Size { get; set; } // Size mặt đồng hồ
        public string Type { get; set; } // Loại đồng hồ: Cơ, Pin, Thể thao...
        public string Style { get; set; } // Phong cách: Casual, Luxury, Sport...
        public string Material { get; set; } // Chất liệu: Thép không gỉ, Vàng, Titanium...
        
        // Các từ khóa và chi tiết bổ sung
        public List<string> Keywords { get; set; } // Các từ khóa quan trọng trong câu hỏi
        public string Detail { get; set; }   // Chi tiết bổ sung
        public bool HasPriceQuery { get; set; } // Có hỏi về giá không
        public bool HasCompareQuery { get; set; } // Có yêu cầu so sánh không
        
        public ChatAnalysis()
        {
            Keywords = new List<string>();
            Intent = "general_question"; // Mặc định là câu hỏi chung
            HasPriceQuery = false;
            HasCompareQuery = false;
        }
    }
}