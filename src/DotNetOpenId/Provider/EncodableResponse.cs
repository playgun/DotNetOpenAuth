using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId;
using DotNetOpenId.RelyingParty;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.Provider {
	internal class EncodableResponse : IEncodable {
		public EncodableResponse(Request request) {
			Request = request;
			Signed = new List<string>();
			Fields = new Dictionary<string, string>();
			if (Protocol.QueryDeclaredNamespaceVersion != null)
				Fields.Add(Protocol.openid.ns, Protocol.QueryDeclaredNamespaceVersion);
		}

		public Request Request { get; private set; }
		public IDictionary<string, string> Fields { get; private set; }
		public List<string> Signed { get; private set; }
		public Protocol Protocol { get { return Request.Protocol; } }
		public bool NeedsSigning {
			get {
				return Request is CheckIdRequest && Signed.Count > 0;
			}
		}

		public void AddField(string nmspace, string key, string value, bool signed) {
			if (!string.IsNullOrEmpty(nmspace)) {
				key = nmspace + "." + key;
			}

			Fields[key] = value;
			if (signed && !Signed.Contains(key)) {
				Signed.Add(key);
			}
		}

		public void AddFields(string nmspace, IDictionary<string, string> fields, bool signed) {
			foreach (var pair in fields) {
				this.AddField(nmspace, pair.Key, pair.Value, signed);
			}
		}

		#region IEncodable Members

		public EncodingType EncodingType {
			get {
				return Request is CheckIdRequest ?
					EncodingType.RedirectBrowserUrl : EncodingType.ResponseBody;
			}
		}

		public IDictionary<string, string> EncodedFields {
			get {
				var nvc = new Dictionary<string, string>();

				foreach (var pair in Fields) {
					if (Request is CheckIdRequest) {
						nvc.Add(Protocol.openid.Prefix + pair.Key, pair.Value);
					} else {
						nvc.Add(pair.Key, pair.Value);
					}
				}

				return nvc;
			}
		}
		public Uri RedirectUrl {
			get {
				if (!(Request is CheckIdRequest)) {
					throw new InvalidOperationException("Encoding to URL is only appropriate on CheckIdRequest requests.");
				}
				CheckIdRequest checkidreq = (CheckIdRequest)Request;
				return checkidreq.ReturnTo;
			}
		}

		#endregion

		public override string ToString() {
			string returnString = String.Format(CultureInfo.CurrentUICulture,
				"Response.NeedsSigning = {0}", this.NeedsSigning);
			foreach (string key in Fields.Keys) {
				returnString += Environment.NewLine + String.Format(CultureInfo.CurrentUICulture,
					"ResponseField[{0}] = '{1}'", key, Fields[key]);
			}
			return returnString;
		}
	}
}
