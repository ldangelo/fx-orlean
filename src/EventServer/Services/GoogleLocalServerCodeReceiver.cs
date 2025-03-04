using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;
using ILogger = Google.Apis.Logging.ILogger;

/*
Copyright 2017 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace EventServer.Services;

/// <summary>
///     OAuth 2.0 verification code receiver that runs a local server on a free port and waits for a call with the
///     authorization verification code.
/// </summary>
public class GoogleLocalServerCodeReceiver : ICodeReceiver
{
    /// <summary>
    ///     Describes the different strategies for the selection of the callback URI.
    ///     127.0.0.1 is recommended, but can't be done in non-admin Windows 7 and 8 at least.
    /// </summary>
    public enum CallbackUriChooserStrategy
    {
        /// <summary>
        ///     Use heuristics to attempt to connect to the recommended URI 127.0.0.1
        ///     but use localhost if that fails.
        /// </summary>
        Default,

        /// <summary>
        ///     Force 127.0.0.1 as the callback URI. No checks are performed.
        /// </summary>
        ForceLoopbackIp,

        /// <summary>
        ///     Force localhost as the callback URI. No checks are performed.
        /// </summary>
        ForceLocalhost
    }

    private static readonly ILogger Logger = ApplicationContext.Logger.ForType<LocalServerCodeReceiver>();

    /// <summary>The call back request path.</summary>
    internal const string LoopbackCallbackPath = "/authorize/";

    /// <summary>Close HTML tag to return the browser so it will close itself.</summary>
    internal const string DefaultClosePageResponse =
        @"<html>
  <head><title>OAuth 2.0 Authentication Token Received</title></head>
  <body>
    Received verification code. You may now close this window.
  </body>
</html>";

    /// <summary>
    ///     Create an instance of <see cref="LocalServerCodeReceiver" />.
    /// </summary>
    public GoogleLocalServerCodeReceiver() : this(DefaultClosePageResponse, CallbackUriChooserStrategy.ForceLocalhost)
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="LocalServerCodeReceiver" />.
    /// </summary>
    /// <param name="closePageResponse">Custom close page response for this instance</param>
    public GoogleLocalServerCodeReceiver(string closePageResponse) :
        this(closePageResponse, CallbackUriChooserStrategy.Default)
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="LocalServerCodeReceiver" />.
    /// </summary>
    /// <param name="closePageResponse">Custom close page response for this instance</param>
    /// <param name="strategy">The strategy to use to determine the callback URI</param>
    public GoogleLocalServerCodeReceiver(string closePageResponse, CallbackUriChooserStrategy strategy)
    {
        _closePageResponse = closePageResponse;
        // Set the instance field of which callback URI to use.
        // An instance field is used to ensure any one instance of this class
        // uses a consistent callback URI.
        _callbackUriTemplate = CallbackUriChooser.Default.GetUriTemplate(strategy);
    }

    // Callback URI used for this instance.
    private readonly string _callbackUriTemplate;

    // Close page response for this instance.
    private readonly string _closePageResponse;

    // Not required in NET45, but present for testing.
    /// <summary>
    ///     An extremely limited HTTP server that can only do exactly what is required
    ///     for this use-case.
    ///     It can only serve localhost; receive a single GET request; read only the query paremters;
    ///     send back a fixed response. Nothing else.
    /// </summary>
    internal class LimitedLocalhostHttpServer : IDisposable
    {
        // RFC7230 recommends supporting a request-line length of at least 8,000 octets
        // https://tools.ietf.org/html/rfc7230#section-3.1.1
        private const int MaxRequestLineLength = 16 * 1024;
        private const int MaxHeadersLength = 64 * 1024;
        private const int NetworkReadBufferSize = 1024;

        private static readonly ILogger Logger = ApplicationContext.Logger.ForType<LimitedLocalhostHttpServer>();

        // Close page response for this instance.
        private readonly string _closePageResponse;
        private readonly CancellationTokenSource _cts;

        private readonly TcpListener _listener;

        private LimitedLocalhostHttpServer(TcpListener listener, string closePageResponse)
        {
            _listener = listener;
            _closePageResponse = closePageResponse;
            _cts = new CancellationTokenSource();
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        }

        public int Port { get; }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
        }

        public static LimitedLocalhostHttpServer Start(string url, string closePageResponse)
        {
            var uri = new Uri(url);
            if (!uri.IsLoopback) throw new ArgumentException($"Url must be loopback, but given: '{url}'", nameof(url));
            var listener = new TcpListener(IPAddress.Loopback, uri.Port);
            return new LimitedLocalhostHttpServer(listener, closePageResponse);
        }

        public async Task<Dictionary<string, string>> GetQueryParamsAsync(CancellationToken cancellationToken = default)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
            using (cts.Token.Register(_listener.Stop))
            {
                try
                {
                    using (var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false))
                    {
                        try
                        {
                            return await GetQueryParamsFromClientAsync(client, cts.Token).ConfigureAwait(false);
                        }
                        catch (ServerException e)
                        {
                            Logger.Warning("{0}", e.Message);
                            throw;
                        }
                    }
                }
                // Cancellation during the `AcceptTcpClientAsync()` call results in an `ObjectDisposedException`.
                // Translate it to the expected cancellation exception.
                catch (ObjectDisposedException) when (cts.IsCancellationRequested)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    // Will never get here, but required to satisfy compiler.
                    throw;
                }
            }
        }

        private async Task<Dictionary<string, string>> GetQueryParamsFromClientAsync(TcpClient client,
            CancellationToken cancellationToken)
        {
            var stream = client.GetStream();
            // NetworkStream.ReadAsync() doesn't honour the cancellation-token (on all platforms),
            // so use workaround
            using (cancellationToken.Register(() => stream.Dispose()))
            {
                var buffer = new byte[NetworkReadBufferSize];
                var bufferOfs = 0;
                var bufferSize = 0;
                Func<Task<char?>> getChar = async () =>
                {
                    if (bufferOfs == bufferSize)
                    {
                        try
                        {
                            bufferSize = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                        }
                        // netcoreapp2.x throws an IOException on stream disposal; others throw ObjectDispoesdException
                        catch (Exception e) when (e is ObjectDisposedException || e is IOException)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }

                        // netcoreapp2.0 on Linux sometimes doesn't throw an exception on stream disposal in ReadAsync,
                        // so check for cancellation afterwards
                        cancellationToken.ThrowIfCancellationRequested();
                        if (bufferSize == 0)
                            // End of stream
                            return null;
                        bufferOfs = 0;
                    }

                    var b = buffer[bufferOfs++];
                    // HTTP headers are generally ASCII, but historically allowed ISO-8859-1.
                    // Non-ASCII bytes should be treated opaquely, not further processed (e.g. as UTF8).
                    return (char)b;
                };

                var requestLine = await ReadRequestLine(getChar).ConfigureAwait(false);
                var requestParams = ValidateAndGetRequestParams(requestLine);
                await WaitForAllHeaders(getChar).ConfigureAwait(false);
                await WriteResponse(stream, cancellationToken).ConfigureAwait(false);

                return requestParams;
            }
        }

        private async Task<string> ReadRequestLine(Func<Task<char?>> getChar)
        {
            var requestLine = new StringBuilder(MaxRequestLineLength);
            do
            {
                if (requestLine.Length >= MaxRequestLineLength)
                    throw new ServerException($"Request line too long: > {MaxRequestLineLength} bytes.");
                var c = await getChar().ConfigureAwait(false);
                if (c == null) throw new ServerException("Unexpected end of network stream reading request line.");
                requestLine.Append(c);
            } while (requestLine.Length < 2 || requestLine[requestLine.Length - 2] != '\r' ||
                     requestLine[requestLine.Length - 1] != '\n');

            requestLine.Length -= 2; // Remove \r\n
            return requestLine.ToString();
        }

        private Dictionary<string, string> ValidateAndGetRequestParams(string requestLine)
        {
            var requestLineParts = requestLine.Split(' ');
            if (requestLineParts.Length != 3)
                throw new ServerException(
                    "Request line ill-formatted. Should be '<request-method> <request-path> HTTP/1.1'");
            var requestVerb = requestLineParts[0];
            if (requestVerb != "GET") throw new ServerException($"Expected 'GET' request, got '{requestVerb}'");
            var requestPath = requestLineParts[1];
            if (!requestPath.StartsWith(LoopbackCallbackPath, StringComparison.Ordinal))
                throw new ServerException(
                    $"Expected request path to start '{LoopbackCallbackPath}', got '{requestPath}'");
            var pathParts = requestPath.Split('?');
            if (pathParts.Length == 1) return new Dictionary<string, string>();
            if (pathParts.Length != 2)
                throw new ServerException($"Expected a single '?' in request path, got '{requestPath}'");
            var queryParams = pathParts[1];
            var result = queryParams.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries).Select(param =>
            {
                var keyValue = param.Split('=');
                if (keyValue.Length > 2) throw new ServerException($"Invalid query parameter: '{param}'");
                var key = WebUtility.UrlDecode(keyValue[0]);
                var value = keyValue.Length == 2 ? WebUtility.UrlDecode(keyValue[1]) : "";
                return new { key, value };
            }).ToDictionary(x => x.key, x => x.value);
            return result;
        }

        private async Task WaitForAllHeaders(Func<Task<char?>> getChar)
        {
            // Looking for an empty line, terminated by \r\n
            var byteCount = 0;
            var lineLength = 0;
            var c0 = '\0';
            var c1 = '\0';
            while (true)
            {
                if (byteCount > MaxHeadersLength)
                    throw new ServerException($"Headers too long: > {MaxHeadersLength} bytes.");
                var c = await getChar().ConfigureAwait(false);
                if (c == null) throw new ServerException("Unexpected end of network stream waiting for headers.");
                c0 = c1;
                c1 = (char)c;
                lineLength += 1;
                byteCount += 1;
                if (c0 == '\r' && c1 == '\n')
                {
                    // End of line
                    if (lineLength == 2) return;
                    lineLength = 0;
                }
            }
        }

        private async Task WriteResponse(NetworkStream stream, CancellationToken cancellationToken)
        {
            var fullResponse = $"HTTP/1.1 200 OK\r\n\r\n{_closePageResponse}";
            var response = Encoding.ASCII.GetBytes(fullResponse);
            await stream.WriteAsync(response, 0, response.Length, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public class ServerException : Exception
        {
            public ServerException(string msg) : base(msg)
            {
            }
        }
    }

    // There is a race condition on the port used for the loopback callback.
    // This is not good, but is now difficult to change due to RedirectUri and ReceiveCodeAsync
    // being public methods.

    private string redirectUri = "http://localhost:8700/";

    /// <inheritdoc />
    public string RedirectUri
    {
        get
        {
            if (string.IsNullOrEmpty(redirectUri))
                redirectUri = string.Format(_callbackUriTemplate, GetRandomUnusedPort());
            return redirectUri;
        }
    }

    /// <inheritdoc />
    public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
        CancellationToken taskCancellationToken)
    {
        var authorizationUrl = url.Build().AbsoluteUri;
        // The listener type depends on platform:
        // * .NET desktop: System.Net.HttpListener
        // * .NET Core: LimitedLocalhostHttpServer (above, HttpListener is not available in any version of netstandard)
        using var listener = StartListener();
        Logger.Debug("Open a browser with \"{0}\" URL", authorizationUrl);
        bool browserOpenedOk;
        try
        {
            browserOpenedOk = OpenBrowser(authorizationUrl);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to launch browser with \"{0}\" for authorization", authorizationUrl);
            throw new NotSupportedException(
                $"Failed to launch browser with \"{authorizationUrl}\" for authorization. See inner exception for details.",
                e);
        }

        if (!browserOpenedOk)
        {
            Logger.Error("Failed to launch browser with \"{0}\" for authorization; platform not supported.",
                authorizationUrl);
            throw new NotSupportedException(
                $"Failed to launch browser with \"{authorizationUrl}\" for authorization; platform not supported.");
        }

        var ret = await GetResponseFromListener(listener, taskCancellationToken).ConfigureAwait(false);

        return ret;
    }

    /// <summary>Returns a random, unused port.</summary>
    private static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        try
        {
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private HttpListener StartListener()
    {
        try
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri);
            listener.Start();
            return listener;
        }
        catch
        {
            CallbackUriChooser.Default.ReportFailure(_callbackUriTemplate);
            throw;
        }
    }

    private async Task<AuthorizationCodeResponseUrl> GetResponseFromListener(HttpListener listener,
        CancellationToken ct)
    {
        HttpListenerContext context;
        // Set up cancellation. HttpListener.GetContextAsync() doesn't accept a cancellation token,
        // the HttpListener needs to be stopped which immediately aborts the GetContextAsync() call.
        using (ct.Register(listener.Stop))
        {
            // Wait to get the authorization code response.
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception) when (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
                // Next line will never be reached because cancellation will always have been requested in this catch block.
                // But it's required to satisfy compiler.
                throw new InvalidOperationException();
            }
            catch
            {
                CallbackUriChooser.Default.ReportFailure(_callbackUriTemplate);
                throw;
            }

            CallbackUriChooser.Default.ReportSuccess(_callbackUriTemplate);
        }

        var coll = context.Request.QueryString;

        // Write a "close" response.
        var bytes = Encoding.UTF8.GetBytes(_closePageResponse);
        context.Response.ContentLength64 = bytes.Length;
        context.Response.SendChunked = false;
        context.Response.KeepAlive = false;
        var output = context.Response.OutputStream;
        await output.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        await output.FlushAsync().ConfigureAwait(false);
        output.Close();
        context.Response.Close();

        // Create a new response URL with a dictionary that contains all the response query parameters.
        return new AuthorizationCodeResponseUrl(coll.AllKeys.ToDictionary(k => k, k => coll[k]));
    }

    /// <summary>
    ///     Open a browser and navigate to a URL.
    /// </summary>
    /// <param name="url">URL to navigate to</param>
    /// <returns>true if browser was launched successfully, false otherwise</returns>
#if NETSTANDARD2_0 || NET6_0_OR_GREATER
    protected virtual bool OpenBrowser(string url)
    {
        // See https://github.com/dotnet/corefx/issues/10361
        // This is best-effort only, but should work most of the time.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // See https://stackoverflow.com/a/6040946/44360 for why this is required
            url = Regex.Replace(url, @"(\\*)" + "\"", @"$1$1\" + "\"");
            url = Regex.Replace(url, @"(\\+)$", @"$1$1");
            Process.Start(new ProcessStartInfo("cmd", $"/c start \"\" \"{url}\"") { CreateNoWindow = true });
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
            return true;
        }

        return false;
    }
#elif NET462_OR_GREATER
        protected virtual bool OpenBrowser(string url)
        {
            Process.Start(url);
            return true;
        }
#else
#error Unsupported target
#endif

    internal class CallbackUriChooser
    {
        /// <summary>Localhost callback URI, expects a port parameter.</summary>
        internal static readonly string CallbackUriTemplateLocalhost = $"http://localhost:{{0}}{LoopbackCallbackPath}";

        /// <summary>127.0.0.1 callback URI, expects a port parameter.</summary>
        internal static readonly string CallbackUriTemplate127001 = $"http://127.0.0.1:{{0}}{LoopbackCallbackPath}";

        private readonly IClock _clock;
        private readonly Func<string, bool> _listenerFailsFor;

        private readonly object _lock = new();

        // TODO: Consider allowing user code to configure this timeout value.
        private readonly TimeSpan _timeout;

        private UriStatistics _localhost;

        // TODO: Consider using a dictionary here. But we only have two templates.
        private UriStatistics _loopbackIp;

        public CallbackUriChooser() :
            this(SystemClock.Default, TimeSpan.FromMinutes(1), FailsHttpListener)
        {
        }

        internal CallbackUriChooser(IClock clock, TimeSpan timeout, Func<string, bool> listenerFailsFor)
        {
            _clock = clock;
            _timeout = timeout;
            _listenerFailsFor = listenerFailsFor;
        }

        public static CallbackUriChooser Default { get; } = new();

        internal string GetUriTemplate(CallbackUriChooserStrategy strategy)
        {
            lock (_lock)
            {
                if (strategy == CallbackUriChooserStrategy.ForceLoopbackIp)
                {
                    // We still want to know what happens, we just won't do the initial check.
                    InitUriStatisticsIfNeeded(ref _loopbackIp, CallbackUriTemplate127001, false);
                    return _loopbackIp.Uri;
                }

                if (strategy == CallbackUriChooserStrategy.ForceLocalhost)
                {
                    // We still want to know what happens, we just won't do the initial check.
                    InitUriStatisticsIfNeeded(ref _localhost, CallbackUriTemplateLocalhost, false);
                    return _localhost.Uri;
                }

                // Listening on 127.0.0.1 is recommended, but can't be done in non-admin Windows 7 & 8 at least.
                // So use some tests/heuristics to maybe listen on localhost instead.

                // If this is the first time that we are called, try with the recommended IP.
                InitUriStatisticsIfNeeded(ref _loopbackIp, CallbackUriTemplate127001, true);
                // We now know something about the loopback IP for sure. Let's see if we can use it. If so,
                // let's return it.
                if (_loopbackIp.CanBeUsed) return _loopbackIp.Uri;

                // If we are here, we know we can't use the loopback IP, either because it failed or because it
                // timed out.

                // Let's try with localhost.
                InitUriStatisticsIfNeeded(ref _localhost, CallbackUriTemplateLocalhost, true);
                // We now know something about localhost for sure. Let's see if we can use it. If so,
                // let's return it.
                if (_localhost.CanBeUsed) return _localhost.Uri;

                // If we are here then we haven't been able to use loopback IP or localhost, either
                // because of failure, or timeout.
                // This is probably bad, but we can still recover if
                // a) Timeouts were because of user inaction.
                // b) Failures were transient.
                // Let's try our best.

                var retriable = _loopbackIp.TotalResets switch
                {
                    // We always prefer the one with less resets.
                    var loopbackResets when loopbackResets < _localhost.TotalResets => _loopbackIp,
                    var loopbackResets when loopbackResets > _localhost.TotalResets => _localhost,
                    // If they have the same amount of resets, then we prefer the one that has timed out
                    // and we prefer loopback if both have timed out.
                    _ when _loopbackIp.IsTimedOut => _loopbackIp,
                    _ when _localhost.IsTimedOut => _localhost,
                    // If they have the same amount of resets and none has timed out (they have failed), then we prefer loopback.
                    _ => _loopbackIp
                };

                retriable.Reset();
                return retriable.Uri;
            }

            void InitUriStatisticsIfNeeded(ref UriStatistics statistics, string uri, bool checkListener)
            {
                if (statistics == null)
                {
                    statistics = new UriStatistics(uri, _timeout, _clock);

                    // If possible, preemptively check that the uri works on this environment.
                    // For instance, the loopback IP fails at least on Windows 7 and 8, for non-admin users.
                    if (checkListener && _listenerFailsFor(statistics.Uri)) statistics.Failed();
                }
            }
        }

        public void ReportSuccess(string uri)
        {
            GetStatisticsFor(uri).Succeeded();
        }

        public void ReportFailure(string uri)
        {
            GetStatisticsFor(uri).Failed();
        }

        private UriStatistics GetStatisticsFor(string uri)
        {
            return uri == CallbackUriTemplate127001 ? _loopbackIp :
                uri == CallbackUriTemplateLocalhost ? _localhost :
                throw new ArgumentOutOfRangeException(nameof(uri));
        }

        private static bool FailsHttpListener(string uri)
        {
            try
            {
                // This listener isn't used for anything except to check if it can listen on the given URI.
                // Hence it is disposed immediately.
                using var listener = new HttpListener();
                listener.Prefixes.Add(string.Format(uri, GetRandomUnusedPort()));
                listener.Start();
            }
            catch (HttpListenerException e) when (e.ErrorCode == 5) // 5: Access denied
            {
                // Access denied for the given URI, report failure.
                return true;
            }
            catch
            {
                // Ignore any errors here, they will re-occur later.
            }

            return false;
        }

        private class UriStatistics
        {
            private readonly IClock _clock;
            private readonly TimeSpan _timeouts;

            public UriStatistics(string uri, TimeSpan timeoutsAfter, IClock clock)
            {
                _timeouts = timeoutsAfter;
                _clock = clock;
                Uri = uri;
                FirstServedAt = new DateTimeOffset(_clock.UtcNow);
                IsKnownToSucceed = false;
                IsKnownToFail = false;
                TotalResets = 0;
            }

            public string Uri { get; }

            public DateTimeOffset FirstServedAt { get; private set; }

            public bool IsKnownToSucceed { get; private set; }

            public bool IsKnownToFail { get; private set; }

            public int TotalResets { get; private set; }

            public bool IsTimedOut =>
                // If we know of success or failure it is not timed out.
                !IsKnownToSucceed && !IsKnownToFail &&
                FirstServedAt.Add(_timeouts) <= _clock.UtcNow;

            public bool CanBeUsed =>
                // If it's known to succeed, even if it has failed, it can be used.
                IsKnownToSucceed ||
                (!IsKnownToFail && !IsTimedOut);

            public void Succeeded()
            {
                IsKnownToSucceed = true;
            }

            public void Failed()
            {
                IsKnownToFail = true;
            }

            public void Reset()
            {
                TotalResets++;
                FirstServedAt = new DateTimeOffset(_clock.UtcNow);
                IsKnownToFail = false;
            }
        }
    }
}