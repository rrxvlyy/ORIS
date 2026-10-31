using System.Net;
using System.Text;
using kontrolnaya.Framework.Attributes;

namespace kontrolnaya.Controllers
{
    [HttpController("feedback")]
    internal class FeedbackController
    {
        
        
        [Get("")]
        public string List(string? author)
        {
            var filtered = FeedbackStorage.Items
                .Where(f => FeedbackStorage.AuthorFilter(f, author));
            var ordered = FeedbackStorage.SortedByMessageLength(filtered).ToList();

            var sb = new StringBuilder();
            sb.Append($@"<form method='get' action='/feedback'>
                Автор: <input name='author' value='{E(author)}'/>
                <button type='submit'>Фильтр</button></form><hr/>");

            sb.Append("<table border='1' cellpadding='4'><tr>" +
                      "<th>Id</th><th>Автор</th><th>Сообщение</th>" +
                      "<th>Длина</th><th>Статус</th></tr>");
            foreach (var f in ordered)
            {
                sb.Append($"<tr><td>{f.Id}</td><td>{E(f.Author)}</td>" +
                          $"<td>{E(f.Message)}</td><td>{f.Message.Length}</td>" +
                          $"<td>{E(f.Status)}</td></tr>");
            }
            sb.Append("</table>");

            sb.Append(@"
                        <hr/><h2>Создать сообщение</h2>
                        <form method='post' action='/feedback/create'>
                        Автор: <input name='author'/><br/>
                        Сообщение: <textarea name='message'></textarea><br/>
                        <button type='submit'>Создать</button>
                        </form>");

            return Page("Обратная связь", sb.ToString());
        }


        
        
        
        
        [Get("details")]
        public string Details(int? id)
        {
            if (id == null) return Error(400, "Не указан id");
            var f = FeedbackStorage.Find(id.Value);
            if (f == null) return Error(404, "Не найдено");

            return Page($"Сообщение #{f.Id}", $@"
                    <p><b>Автор:</b> {E(f.Author)}</p>
                    <p><b>Сообщение:</b><br/>{E(f.Message)}</p>
                    <p><b>Статус:</b> {E(f.Status)}</p>
                    <p><b>Длина:</b> {f.Message.Length}</p>
                    <p><a href='/feedback'>Назад</a></p>");
        }

        
        
        
        
        
        
        
        [Post("create")]
        public string Create(string? author, string? message)
        {
            if (!FeedbackStorage.IsAuthor(author) ||
                !FeedbackStorage.IsMessage(message))
                return Error(400, "Автор 2–60, сообщение 5–300 символов");

            FeedbackStorage.Items.Add(new Feedback
            {
                Id = FeedbackStorage.NextId(),
                Author = author!,
                Message = message!,
                Status = "pending"
            });

            return Redirect("/feedback");
        }

        
        
        
        
        
        
        [Put("update")]
        public string Update(int? id, string? author, string? message, string? status)
        {
            if (id == null) return Error(400, "Не указан id");
            var f = FeedbackStorage.Find(id.Value);
            if (f == null) return Error(404, "Не найдено");
            
            if (author == null || message == null || status == null)
                return Error(400, "Нужны author, message, status");

            if (!FeedbackStorage.IsAuthor(author) ||
                !FeedbackStorage.IsMessage(message) ||
                !FeedbackStorage.IsStatus(status))
                return Error(400, "Некорректные данные");

            f.Author = author;
            f.Message = message;
            f.Status = status;

            return NoContent();
        }
        
        
        
        

        private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

        private static string Page(string title, string body) =>
            $"<!DOCTYPE html><html><head><meta charset='utf-8'>" +
            $"<title>{E(title)}</title></head><body>" +
            $"<h1>{E(title)}</h1>{body}</body></html>";
        
        private static string Error(int code, string msg) =>
            $"__STATUS__{code}__{msg}";

        private static string Redirect(string url) =>
            $"__STATUS__303__{url}";

        private static string NoContent() => "__STATUS__204__";
    }
}