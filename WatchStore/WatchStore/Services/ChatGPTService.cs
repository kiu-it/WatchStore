using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WatchStore.Services
{
    public class ChatGPTService
    {
        private readonly string apiKey = ""; // Thay bằng API Key thật

        public async Task<string> AskChatGPT(string message)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var requestData = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = @"Bạn là chatbot tư vấn của cửa hàng đồng hồ AnhKiuWatch. 
                            Hãy trả lời mọi câu hỏi một cách chuyên nghiệp, ngắn gọn và hữu ích.
                            Đối với câu hỏi về đồng hồ, tập trung vào việc cung cấp thông tin chính xác và hữu ích.
                            Đối với câu hỏi không liên quan đến đồng hồ, trả lời như một trợ lý thông minh và thân thiện.
                            Luôn giữ giọng điệu lịch sự và chuyên nghiệp." },
                        new { role = "user", content = message }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic json = JsonConvert.DeserializeObject(responseString);
                return json.choices[0].message.content.ToString();
            }
        }
    }
}
