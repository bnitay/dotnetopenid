using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;
using System.Collections.Specialized;

namespace DotNetOpenId.Consumer
{
    public class AuthRequest
    {
        public enum Mode
        {
            IMMEDIATE,
            SETUP
        }

		private string token;
		private Dictionary<string, string> extraArgs;
		private Dictionary<string, string> returnToArgs;
		private Association assoc;
		private ServiceEndpoint endpoint;

		public AuthRequest(string token, Association assoc, ServiceEndpoint endpoint)
		{
			this.token = token;
			this.assoc = assoc;
			this.endpoint = endpoint;

			this.extraArgs = new Dictionary<string, string>();
			this.returnToArgs = new Dictionary<string, string>();
		}

        public string Token
        {
            get { return this.token; }
            set { this.token = value; }
        }

        public IDictionary<string, string> ExtraArgs
        {
            get { return this.extraArgs; }
        }

        public IDictionary<string, string> ReturnToArgs
		{
			get { return this.returnToArgs; }
		}

        public Uri CreateRedirect(string trustRoot, Uri returnTo, Mode mode)
        {
            string modeStr = String.Empty;
            if (mode == Mode.IMMEDIATE)
                modeStr = QueryStringArgs.Modes.checkid_immediate;
            else if (mode == Mode.SETUP)
                modeStr = QueryStringArgs.Modes.checkid_setup;

            UriBuilder returnToBuilder = new UriBuilder(returnTo);
            UriUtil.AppendQueryArgs(returnToBuilder, this.ReturnToArgs);

            var qsArgs = new Dictionary<string, string>();

            qsArgs.Add(QueryStringArgs.openid.mode, modeStr);
            qsArgs.Add(QueryStringArgs.openid.identity, this.endpoint.ServerId.AbsoluteUri); //TODO: breaks the Law of Demeter
            qsArgs.Add(QueryStringArgs.openid.return_to, returnToBuilder.ToString());
            qsArgs.Add(QueryStringArgs.openid.trust_root, trustRoot);

            if (this.assoc != null)
                qsArgs.Add(QueryStringArgs.openid.assoc_handle, this.assoc.Handle); // !!!!

            UriBuilder redir = new UriBuilder(this.endpoint.ServerUrl);

            UriUtil.AppendQueryArgs(redir, qsArgs);
            UriUtil.AppendQueryArgs(redir, this.ExtraArgs);

            return new Uri(redir.ToString(), true);
        }

    }
}


