using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace WordSearcher
{
    public class Api
    {
        private HttpClient client = new HttpClient();
        private Uri host;
        private string token;

        public Api(Uri host, string token)
        {
            this.host = host;
            this.token = token;
            this.client.DefaultRequestHeaders.Add("Authorization", $"token {this.token}");
        }

        public HttpResponseMessage start(out bool?[][] initialRect, bool test = false)
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/game/start";

            if (test) url.Query = "test=true";

            var resp = this.client.PostAsync(url.Uri, new ByteArrayContent(new byte[0]))
                .GetAwaiter()
                .GetResult();

            initialRect = this.parseRectResponse(resp);

            return resp;
        }

        public HttpResponseMessage finish()
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/game/finish";
            return this.client.PostAsync(url.Uri, new ByteArrayContent(new byte[0]))
                .GetAwaiter()
                .GetResult();
        }

        public HttpResponseMessage stats(out string result)
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/game/stats";

            var resp = this.client.GetAsync(url.Uri)
                .GetAwaiter()
                .GetResult();

            result = this.getResponseBody(resp);

            return resp;
        }

        public HttpResponseMessage words(string[] words, out string result)
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/words";

            var reqBody = '[' + String.Join(", ", words.Select((string w) => $"\"{w}\"")) + ']';
            var content = new StringContent(reqBody, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resp = this.client.PostAsync(url.Uri, content)
                .GetAwaiter()
                .GetResult();

            result = this.getResponseBody(resp);

            return resp;
        }

        public HttpResponseMessage moveUp(out bool?[][] rect)
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/move/up";

            var resp = this.client.PostAsync(url.Uri, new ByteArrayContent(new byte[0]))
                .GetAwaiter()
                .GetResult();

            rect = this.parseRectResponse(resp);

            return resp;
        }

        public HttpResponseMessage moveDown(out bool?[][] rect)
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/move/down";

            var resp = this.client.PostAsync(url.Uri, new ByteArrayContent(new byte[0]))
                .GetAwaiter()
                .GetResult();

            rect = this.parseRectResponse(resp);

            return resp;
        }

        public HttpResponseMessage moveLeft(out bool?[][] rect)
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/move/left";

            var resp = this.client.PostAsync(url.Uri, new ByteArrayContent(new byte[0]))
                .GetAwaiter()
                .GetResult();

            rect = this.parseRectResponse(resp);

            return resp;
        }

        public HttpResponseMessage moveRight(out bool?[][] rect)
        {
            var url = new UriBuilder(this.host);
            url.Path = "/task/move/right";

            var resp = this.client.PostAsync(url.Uri, new ByteArrayContent(new byte[0]))
                .GetAwaiter()
                .GetResult();

            rect = this.parseRectResponse(resp);

            return resp;
        }

        private string getResponseBody(HttpResponseMessage resp)
        {
            return resp.Content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
        }

        private bool?[][] parseRectResponse(HttpResponseMessage resp)
        {
            var str = this.getResponseBody(resp);

            var lines = str.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            return Utils.createTableFromLines(lines);
        }
    }
}
