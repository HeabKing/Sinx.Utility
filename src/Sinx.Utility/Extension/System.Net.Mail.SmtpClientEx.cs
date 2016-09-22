#if NET451
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Sinx.Utility.Extension
{
    public static class SmtpClientEx
    {
        public enum ServiceProvider
        {
            // ReSharper disable once InconsistentNaming
            TecentQQ,
            WangYi163
        }

        /// <summary>
        /// 创建指定的Email客户端
        /// </summary>
        /// <param name="smtp"></param>
        /// <param name="userName">邮箱号: Sample@Domain.com</param>
        /// <param name="pwd">密码</param>
        /// <param name="serviceProvider">服务提供商</param>
        /// <returns></returns>
        public static IMessageService Create(this SmtpClient smtp, string userName, string pwd,
            ServiceProvider serviceProvider = ServiceProvider.TecentQQ)
        {
            return Create(userName, pwd, serviceProvider);
        }

        /// <summary>
        /// 创建指定的Email客户端
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IMessageService Create(string userName, string pwd, ServiceProvider serviceProvider = ServiceProvider.TecentQQ)
        {
            switch (serviceProvider)
            {
                case ServiceProvider.TecentQQ:
                    return new EmailClient(userName.Split('@')[0], pwd, userName, "smtp.qq.com", true, 587);
                case ServiceProvider.WangYi163:
                    return new EmailClient(userName, pwd, userName, "smtp.163.com", false, 25);
                default:
                    throw new ArgumentException(nameof(serviceProvider));
            }
        }
    }

    /// <summary>
    /// 消息类 - 设置成接口是防止误用Message实例导致转成EmailMessage等派生类的时候出错
    /// </summary>
    public interface IMessage
    {
        // 内容
        string Msg { get; set; }
    }

    /// <summary>
    /// 信息发送服务接口
    /// </summary>
    /// <remarks>何士雄 2016-05-13</remarks>
    public interface IMessageService
    {
        /// <summary>
        /// 发送信息的接口
        /// </summary>
        /// <remarks>何士雄 2016-05-13</remarks>
        /// <param name="to">向谁发送(一个/几个)</param>
        /// <param name="msg">发送的内容</param>
        /// <returns></returns>
        bool SendMessage(IMessage msg, params string[] to);
    }

    /// <summary>
    /// 消息类
    /// </summary>
    public class EmailMessage : IMessage
    {
        public EmailMessage(string msg, string subject = null)
        {
            Msg = msg;
            Subject = subject;
        }
        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Msg { get; set; }
    }

    /// <summary>
    /// 使用邮件服务发送消息
    /// </summary>
    /// <remarks>何士雄 2016-05-13</remarks>
    public class EmailClient : IMessageService
    {
        /// <summary>
        /// 发送邮件的客户端
        /// </summary>
        private readonly SmtpClient _smtpClient = new SmtpClient();
        /// <summary>
        /// 代表一个邮件
        /// </summary>
        private readonly MailMessage _mailMessage = new MailMessage();

        /// <summary>
        /// 对客户端进行初始化
        /// </summary>
        /// <param name="userName">用户名, QQ邮箱不用加@qq.com</param>
        /// <param name="pwd">密码: 使用授权码</param>
        /// <param name="from">发件人邮箱</param>
        /// <param name="host">发件人邮箱服务器地址</param>
        /// <param name="emableSsl"></param>
        /// <param name="port">发件人邮箱服务器端口号</param>
        public EmailClient(string userName, string pwd, string from, string host, bool emableSsl, int port)
        {
            // 证书 - 用户名 + 密码
            _smtpClient.Credentials = new System.Net.NetworkCredential(userName, pwd);
            // 邮件服务器地址
            _smtpClient.Host = host;
            // 是否使用SSL加密
            _smtpClient.EnableSsl = emableSsl;
            // 使用的端口号
            _smtpClient.Port = port;

            // 发件人邮箱
            _mailMessage.From = new MailAddress(from);
            // 主题内容使用的编码
            _mailMessage.SubjectEncoding = Encoding.UTF8;
            // 正文使用的编码
            _mailMessage.BodyEncoding = Encoding.Default;
            // 优先级
            _mailMessage.Priority = MailPriority.High;
            // 邮件正文是否为Html格式
            _mailMessage.IsBodyHtml = true;
            // 附件
            //_mailMessage.Attachments.Add(new Attachment("fullpath"));
        }

        public bool SendMessage(IMessage msg, params string[] to)
        {
            //向收件人地址集合添加邮件地址
            foreach (var m in to)
            {
                _mailMessage.To.Add(m);
            }
            var emailMessage = msg as EmailMessage;
            // 添加主题
            _mailMessage.Subject = emailMessage?.Subject;
            // 添加正文
            _mailMessage.Body = emailMessage?.Msg ?? "";

            try
            {
                //将邮件发送到SMTP邮件服务器
                _smtpClient.Send(_mailMessage);
                Debug.WriteLine("发送邮件成功");
                return true;
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                Debug.WriteLine(ex);
                return false;
            }

        }
    }
}
#endif