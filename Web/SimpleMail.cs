using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;
using System.Timers;

namespace Paulus.Web
{
    [Serializable]
    public class MailMessageEventArgs : EventArgs
    {
        public MailMessageEventArgs(MailMessage mailMessage) { _mailMessage = MailMessage; }

        private MailMessage _mailMessage;
        public MailMessage MailMessage { get { return _mailMessage; } }
    }

    [Serializable]
    public class MailMessageFailedRecipientEventArgs : EventArgs
    {
        public MailMessageFailedRecipientEventArgs(MailAddress failedRecipient)
        {
            _failedRecipient = failedRecipient;
        }

        private MailAddress _failedRecipient;
        public MailAddress FailedRecipient { get { return _failedRecipient; } }
    }

    public class SimpleMail : MailMessage
    {
        #region Mail addresses
        //contains all the mail addresses (to, cc, bcc) and is used in case of error
        //updated before sending the command

        //set at the Send() function
        protected Dictionary<string, MailAddress> _mailAddresses;
        #endregion

        #region Content

        protected string _htmlBody;
        //eg. "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
        //body += "<HTML><HEAD><META http-equiv=Content-Type content=\"text/html; charset=iso-8859-1\">";
        //body += "</HEAD><BODY><DIV><FONT face=Arial color=#ff0000 size=2>this is some HTML text";
        //body += "</FONT></DIV></BODY></HTML>";
        public string HTMLBody { get { return _htmlBody; } set { _htmlBody = value; } }
        private bool hasHTMLBody { get { return !string.IsNullOrEmpty(_htmlBody); } }
        //public void SetHTMLBodyFromFile(string path, Encoding encoding)
        //{
        //    _htmlBody = new StreamReader(path, encoding).ReadToEnd();
        //}
        //public void SetHTMLBodyFromFile(string path) { SetHTMLBodyFromFile(path, Encoding.Default); }
        protected Encoding _htmlBodyEncoding;
        public Encoding HTMLBodyEncoding
        {
            get { return _htmlBodyEncoding; }
            set
            {
                if (value == null) throw new ArgumentNullException("HTMLBodyEncoding", "HTML body encoding cannot be null.");
                _htmlBodyEncoding = value;
            }
        }

        protected string _textBody;
        public string TextBody { get { return _textBody; } set { _textBody = value; } }
        private bool hasTextBody { get { return !string.IsNullOrEmpty(_textBody); } }
        //public void SetTextBodyFromFile(string path,Encoding encoding) {_textBody = new StreamReader(path, encoding).ReadToEnd();}
        //public void SetTextBodyFromFile(string path) { SetTextBodyFromFile(path, Encoding.Default);}
        protected Encoding _textBodyEncoding;
        public Encoding TextBodyEncoding
        {
            get { return _textBodyEncoding; }
            set
            {
                if (value == null) throw new ArgumentNullException("TextBodyEncoding", "Text body encoding cannot be null.");

                _textBodyEncoding = value;
            }
        }
        #endregion

        #region SMTP
        protected SmtpClient _SMTP;
        public SmtpClient SMTP
        {
            get { return _SMTP; }
            set
            {
                if (value == null) throw new ArgumentNullException("SMTP", "SMTP cannot be null.");
                if (_SMTP != value)
                {
                    if (_SMTP != null) _SMTP.SendCompleted -= SMTP_SendCompleted;
                    _SMTP = value;
                    _SMTP.SendCompleted += new SendCompletedEventHandler(SMTP_SendCompleted);
                }
            }
        }

        private void SMTP_SendCompleted(object sender, global::System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Exception exc = e.Error;
            if (exc == null)
                OnSentSuccessfully();
            else if (exc is SmtpException)
                OnMailServerError();
            else if (exc is SmtpFailedRecipientsException)
            {
                SmtpFailedRecipientsException s = exc as SmtpFailedRecipientsException;
                foreach (SmtpFailedRecipientException innerException in s.InnerExceptions)
                {
                    SmtpStatusCode status = innerException.StatusCode;
                    string recipient = innerException.FailedRecipient;
                    if (status == SmtpStatusCode.MailboxBusy)
                        OnRecipientMailboxBusy(recipient);
                    else
                        OnRecipientMailboxUnavailable(recipient);
                }
            }
        }
        #endregion

        public void Send()
        {
            //validate
            if (From == null) throw new ArgumentNullException("From", "From cannot be null.");
            if (To.Count == 0 && CC.Count == 0 && Bcc.Count == 0)
                throw new ArgumentException("To, CC and BCC contain no recipients at all.");

            //initialize message

            #region Collect and set addresses
            //used to store the name,e-mail combination and is later to be used by error if any errors occur
            _mailAddresses = new Dictionary<string, MailAddress>();
            foreach (MailAddress to in To)
                _mailAddresses.Add(to.Address, to);
            foreach (MailAddress cc in CC)
                _mailAddresses.Add(cc.Address, cc);
            foreach (MailAddress bcc in Bcc)
                _mailAddresses.Add(bcc.Address, bcc);
            #endregion

            #region Set content

            if (hasTextBody)
            {
                IsBodyHtml = false;
                Body = _textBody;
                if (_textBodyEncoding != null) BodyEncoding = _textBodyEncoding; //set the body encoding ?

                if (hasHTMLBody)
                {
                    global::System.Net.Mail.AlternateView view;
                    if (_htmlBodyEncoding == null)
                    {
                        global::System.Net.Mime.ContentType mimeType = new global::System.Net.Mime.ContentType("text/html");
                        view = global::System.Net.Mail.AlternateView.CreateAlternateViewFromString(_htmlBody, mimeType);
                    }
                    else
                        view = AlternateView.CreateAlternateViewFromString(_htmlBody, _htmlBodyEncoding, "text/html");

                    AlternateViews.Add(view);
                }
            }
            else if (hasHTMLBody)
            {
                IsBodyHtml = true;
                Body = _htmlBody;
                if (_htmlBodyEncoding != null) BodyEncoding = _htmlBodyEncoding;
            }

            #endregion

            //try to send the e-mail asynchronously
            try
            {
                _SMTP.SendAsync(this, null);
            }
            catch
            {
                OnMailServerError();
            }
        }

        #region Events
        public event EventHandler SentSuccessfully;
        protected void OnSentSuccessfully()
        {
            var handler = SentSuccessfully;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //if the error is smtp exception then the mail cannot be sent in general
        public event EventHandler MailServerError;
        protected void OnMailServerError()
        {
            var handler = MailServerError;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public EventHandler<MailMessageFailedRecipientEventArgs> RecipientMailboxBusy;
        protected void OnRecipientMailboxBusy(string mailAddress)
        {
            var handler = RecipientMailboxBusy;
            if (handler != null) handler(this, new MailMessageFailedRecipientEventArgs(_mailAddresses[mailAddress]));
        }

        public EventHandler<MailMessageFailedRecipientEventArgs> RecipientMailboxUnavailable;
        protected void OnRecipientMailboxUnavailable(string mailAddress)
        {
            var handler = RecipientMailboxUnavailable;
            if (handler != null) handler(this, new MailMessageFailedRecipientEventArgs(_mailAddresses[mailAddress]));
        }
        #endregion



    }
}
