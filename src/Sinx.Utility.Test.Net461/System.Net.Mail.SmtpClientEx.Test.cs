using System;
using System.Net.Mail;
using Sinx.Utility.Extension;
using Xunit;

namespace Sinx.Utility.Test.Net461
{
    public class SmtpClientExTest
    {
        [Fact]
        public void CreateClientTest()
        {
            var s = SmtpClientEx.Create("HeabKing@qq.com", "pwuomsefcevacbeg");
            s.SendMessage(new EmailMessage($"<h1>Just For Test</h1> ---- {DateTime.Now}"), "394899990@qq.com");
        }
    }
}
