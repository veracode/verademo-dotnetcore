using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using VeraDemoNet.CustomAuthentication;
using VeraDemoNet.Models;

namespace VeraDemoNet.Controllers
{
    public class ToolsController : AuthControllerBase
    {
        protected readonly log4net.ILog logger;

        public ToolsController()
        {
            logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);    
        }

        [HttpGet, ActionName("Tools")]
        public ActionResult GetTools()
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            return View(new ToolViewModel());
        }

        [HttpPost]
        public ActionResult Ping(string host)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var viewModel = new ToolViewModel();

            if (string.IsNullOrEmpty(host))
            {
                return View("Tools", viewModel);
            }

            var output = new StringBuilder();
            try {
                // START BAD CODE
                var fileName = "cmd.exe";
                var arguments = "/c ping " + host;
                // END BAD CODE

                var proc = CreateStdOutProcess(fileName, arguments);

                proc.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { output.AppendLine(e.Data); };
                proc.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e) { output.AppendLine(e.Data); };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
            catch (Exception ex) 
            {
                logger.Error(ex);
            }

            viewModel.Host = host;
            viewModel.PingResult = output.ToString();

            return View("Tools", viewModel);
        }

        [HttpPost]
        public ActionResult Fortune(string fortuneFile)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var output = new StringBuilder();

            if (string.IsNullOrEmpty(fortuneFile)) 
            {
                fortuneFile = "funny.txt";
            }

            try
            {
                // START BAD CODE
                var fileName = "cmd.exe";
                var arguments = "/c " + HostingEnvironment.MapPath("~/Resources/bin/fortune-go.exe") + " " + HostingEnvironment.MapPath("~/Resources/bin/" + fortuneFile);
                // END BAD CODE

                var proc = CreateStdOutProcess(fileName, arguments);

                proc.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { output.Append(e.Data); };
                proc.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e) { output.Append(e.Data); };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return View("Tools", new ToolViewModel
            {
               FortuneResult = output.ToString()
            });
        }

        private static Process CreateStdOutProcess(string filename, string arguments)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            return proc;
        }
    }
}