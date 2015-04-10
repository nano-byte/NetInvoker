using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;
using System.Windows.Forms;
using NanoByte.NetInvoker.Properties;

namespace NanoByte.NetInvoker
{
    public partial class MainForm : Form
    {
        private readonly HttpListener _listener = new HttpListener
        {
            AuthenticationSchemes = AuthenticationSchemes.Basic,
            Prefixes = {"http://*:" + Settings.Default.Port + "/"}
        };

        public MainForm()
        {
            InitializeComponent();

            HandleCreated += async delegate
            {
                _listener.Start();
                while (_listener.IsListening)
                {
                    try
                    {
                        HandleRequest(await _listener.GetContextAsync());
                    }
                    catch (HttpListenerException)
                    {}
                }
            };

            FormClosing += delegate { _listener.Stop(); };
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            var identity = (HttpListenerBasicIdentity)context.User.Identity;
            if (identity.Name != Settings.Default.UserName || identity.Password != Settings.Default.Password)
            {
                context.Respond(HttpStatusCode.Unauthorized, "User name or password invalid");
                return;
            }

            NameValueCollection parameters;
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    if (context.Request.RawUrl == "/")
                    {
                        context.Respond(HttpStatusCode.OK, GetHtml());
                        return;
                    }

                    parameters = context.Request.QueryString;
                    break;

                case "POST":
                    string input;
                    using (var reader = new StreamReader(context.Request.InputStream))
                        input = reader.ReadToEnd();
                    parameters = HttpUtility.ParseQueryString(input);
                    break;

                default:
                    context.Respond(HttpStatusCode.MethodNotAllowed, "Use GET or POST");
                    return;
            }

            string fileName = parameters["fileName"];
            string arguments = parameters["arguments"];

            if (string.IsNullOrEmpty(fileName))
            {
                context.Respond(HttpStatusCode.BadRequest, GetHtml());
                return;
            }

            try
            {
                Process.Start(fileName, arguments);
            }
                #region Error handling
            catch (IOException ex)
            {
                context.Respond(HttpStatusCode.InternalServerError, ex.Message);
                return;
            }
            catch (Win32Exception ex)
            {
                context.Respond(HttpStatusCode.InternalServerError, ex.Message);
                return;
            }
            #endregion
            context.Respond(HttpStatusCode.NoContent);
        }

        private static string GetHtml()
        {
            string serviceUri = "http://" + Environment.MachineName + ":8888/";
            return
                "<!DOCTYPE html><html><head><title>NetInvoker</title></head>" +
                "<body><h1>NetInvoker</h1>" +
                "<form action='" +serviceUri + "' method='post' spellcheck='false'>" +
                "<table>" +
                "<tr><td><label for='fileName'>File name:</label></td><td><input name='fileName' type='text' required></td></tr>" +
                "<tr><td><label for='arguments'>Arguments:</label></td><td><input name='arguments' type='text'></td></tr>" +
                "</table>" +
                "<input type='submit' value='Run'></form>" +
                "<a href='javascript:window.location.href=\"" + serviceUri + "?fileName=\"+encodeURIComponent(document.URL);'>Bookmarklet</a>" +
                "</body></html>";
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
            if (!IsHandleCreated) CreateHandle();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
