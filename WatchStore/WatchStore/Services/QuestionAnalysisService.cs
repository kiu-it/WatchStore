using System;
using System.Collections.Generic;
using System.Linq;
using WatchStore.Models;

namespace WatchStore.Services
{
    public class QuestionAnalysisService
    {
        private readonly List<string> productKeywords = new List<string>
        {
            "đồng hồ", "watch", "đồng hồ đeo tay", "đồng hồ nam", "đồng hồ nữ"
        };

        private readonly List<string> brandKeywords = new List<string>
        {
            "orient", "tissot", "citizen", "casio", "seiko", "rolex", "omega"
        };

        private readonly List<string> typeKeywords = new List<string>
        {
            "cơ", "pin", "automatic", "quartz", "thể thao", "smart watch"
        };

        private readonly List<string> styleKeywords = new List<string>
        {
            "casual", "luxury", "sport", "classic", "vintage", "modern"
        };

        private readonly List<string> materialKeywords = new List<string>
        {
            "thép không gỉ", "vàng", "titanium", "da", "cao su", "dây da"
        };

        private readonly List<string> priceKeywords = new List<string>
        {
            "giá", "price", "cost", "đắt", "rẻ", "tiền", "bao nhiêu"
        };

        private readonly List<string> compareKeywords = new List<string>
        {
            "so sánh", "compare", "khác nhau", "khác biệt", "tốt hơn", "hơn"
        };

        public ChatAnalysis AnalyzeQuestion(string question)
        {
            var analysis = new ChatAnalysis();
            question = question.ToLower();

            // Phân tích ý định chính
            DetermineIntent(question, analysis);

            // Phân tích loại câu hỏi
            DetermineQueryType(question, analysis);

            // Trích xuất thông tin chi tiết
            ExtractProductDetails(question, analysis);

            // Kiểm tra câu hỏi về giá
            analysis.HasPriceQuery = priceKeywords.Any(k => question.Contains(k));

            // Kiểm tra yêu cầu so sánh
            analysis.HasCompareQuery = compareKeywords.Any(k => question.Contains(k));

            return analysis;
        }

        private void DetermineIntent(string question, ChatAnalysis analysis)
        {
            if (productKeywords.Any(k => question.Contains(k)))
            {
                analysis.Intent = "product_query";
                if (priceKeywords.Any(k => question.Contains(k)))
                {
                    analysis.QueryType = "price_info";
                }
                else if (compareKeywords.Any(k => question.Contains(k)))
                {
                    analysis.QueryType = "compare";
                }
            }
            else
            {
                analysis.Intent = "general_question";
            }
        }

        private void DetermineQueryType(string question, ChatAnalysis analysis)
        {
            if (question.Contains("giới thiệu") || question.Contains("thông tin"))
            {
                analysis.QueryType = "info";
            }
            else if (question.Contains("tư vấn") || question.Contains("gợi ý"))
            {
                analysis.QueryType = "recommend";
            }
            else if (analysis.QueryType == null)
            {
                analysis.QueryType = "search";
            }
        }

        private void ExtractProductDetails(string question, ChatAnalysis analysis)
        {
            // Trích xuất thương hiệu
            analysis.Brand = brandKeywords.FirstOrDefault(b => question.Contains(b));

            // Trích xuất loại đồng hồ
            analysis.Type = typeKeywords.FirstOrDefault(t => question.Contains(t));

            // Trích xuất phong cách
            analysis.Style = styleKeywords.FirstOrDefault(s => question.Contains(s));

            // Trích xuất chất liệu
            analysis.Material = materialKeywords.FirstOrDefault(m => question.Contains(m));

            // Xác định giới tính
            if (question.Contains("nam"))
                analysis.Gender = "Nam";
            else if (question.Contains("nữ"))
                analysis.Gender = "Nữ";

            // Trích xuất khoảng giá
            ExtractPriceRange(question, analysis);

            // Thu thập từ khóa quan trọng
            CollectKeywords(question, analysis);
        }

        private void ExtractPriceRange(string question, ChatAnalysis analysis)
        {
            // Tìm các con số trong câu hỏi
            var numbers = question.Split(' ')
                .Where(w => w.Any(char.IsDigit))
                .Select(w => {
                    double num;
                    if (double.TryParse(w.Replace(".", "").Replace(",", ""), out num))
                        return num;
                    return 0;
                })
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            if (numbers.Count >= 2)
            {
                analysis.PriceMin = numbers[0];
                analysis.PriceMax = numbers[1];
            }
            else if (numbers.Count == 1)
            {
                if (question.Contains("dưới") || question.Contains("ít hơn"))
                {
                    analysis.PriceMax = numbers[0];
                }
                else if (question.Contains("trên") || question.Contains("hơn"))
                {
                    analysis.PriceMin = numbers[0];
                }
                else
                {
                    analysis.PriceMax = numbers[0];
                }
            }
        }

        private void CollectKeywords(string question, ChatAnalysis analysis)
        {
            analysis.Keywords.AddRange(productKeywords.Where(k => question.Contains(k)));
            analysis.Keywords.AddRange(brandKeywords.Where(k => question.Contains(k)));
            analysis.Keywords.AddRange(typeKeywords.Where(k => question.Contains(k)));
            analysis.Keywords.AddRange(styleKeywords.Where(k => question.Contains(k)));
            analysis.Keywords.AddRange(materialKeywords.Where(k => question.Contains(k)));
        }
    }
}