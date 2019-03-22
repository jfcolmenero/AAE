using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AuthorizationContext
{
    public class AuthorizationContext
    {
        #region Attributes

        HashSet<string> _UserMasterRecord;
        string _Secret;

        #endregion

        #region Properties

        public string UserId { get; private set; }

        #endregion

        #region Public Methods

        public AuthorizationContext(string userId, string secret, List<string> keyList)
        {
            UserId = userId;
            _Secret = secret;
            _UserMasterRecord = new HashSet<string>(keyList);
        }

        #region Authority Check Methods

        public bool Check(string authorizationObjectId, params string[] args)
        {
            Dictionary<string, string> authorizationInstance = new Dictionary<string, string>();

            var argsList = args.ToList();

            argsList.Sort();

            foreach (var param in argsList)
                if (param.Count(x => x == '=') == 1)
                    authorizationInstance.Add(param.Split('=')[0], param.Split('=')[1]);
                else
                    throw new Exception("You can only set one symbol '=' for the authorization instance values.");

            var key = GetHashSha256(UserId + authorizationObjectId + JsonConvert.SerializeObject(authorizationInstance) + _Secret);

            return _UserMasterRecord.Contains(key);

        } // Check

        #endregion

        #endregion

        #region Private Methods

        private string Fingerprint()
        {

            var authorizationList = _UserMasterRecord.ToList();

            authorizationList.Sort();

            StringBuilder sb = new StringBuilder();

            foreach (var str in authorizationList)
                sb.Append(str);


            return GetHashSha256(sb.ToString());

        } // GetFingerprint


        private string GetHashSha256(string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            var base64String = System.Convert.ToBase64String(hash);

            return base64String;

        } // GetHashSha256

        #endregion

    } // AuthorizationContext

}