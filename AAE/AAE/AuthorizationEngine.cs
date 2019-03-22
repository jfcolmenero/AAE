using AAE.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace AAE
{
    /// <summary>
    /// All types handled by the Authorization Engine are string or List<string>.
    /// This would make the Auth Engine fully portable
    /// Complex types should be implemented in the pluggable repositories
    /// </summary>
    public sealed class AuthorizationEngine
    {
        #region Private Attributes

        private IAuthorizationRepository _authRepository;
        private const string _ALL = "*";
        private const int _RecursiveStackSize = 39452672; // 32 MBytes

        #endregion

        #region Public Methods

        /// <summary>
        ///  THIS CONSTRUCTOR MUST BE CHANGED TO USE IoC
        /// </summary>
        /// <param name="authRepository"></param>
        public AuthorizationEngine(IAuthorizationRepository authRepository)
        {
            _authRepository = authRepository;

        } // Constructor

        # region Authorization Group

        public void CreateAuthorizationGroup(string authorizationGroupId)
        {
            _authRepository.CreateAuthorizationGroup(authorizationGroupId);

        } // CreateAuthorizationGroup

        public bool AuthorizationGroupExists(string authorizationGroupId)
        {
            return _authRepository.AuthorizationGroupExists(authorizationGroupId);

        } // AuthorizationGroupExists

        public void DeleteAuthorizationGroup(string authorizationGroupId)
        {
            _authRepository.DeleteAuthorizationGroup(authorizationGroupId);

        } // DeleteAuthorizationGroup

        #endregion

        #region Field

        public void CreateField(string fieldId, List<string> valueList)
        {
            valueList.Add(_ALL);

            var noDuplicatesList = valueList.Distinct().ToList();

            _authRepository.CreateField(fieldId, noDuplicatesList);

        } // CreateField

        public void UpdateFieldValues(string fieldId, List<string> valueList)
        {
            var noDuplicatesList = valueList.Distinct().ToList();

            _authRepository.UpdateFieldValues(fieldId, noDuplicatesList);

        } // UpdateFieldValues

        public void AppendFieldValues(string fieldId, List<string> valueList)
        {

            var appendList = _authRepository.GetFieldValues(fieldId);

            appendList.AddRange(valueList);

            var noDuplicatesList = appendList.Distinct().ToList();

            _authRepository.UpdateFieldValues(fieldId, noDuplicatesList);

        } // AppendFieldValues

        public List<string> ReadFieldValues(string fieldId)
        {
            return _authRepository.GetFieldValues(fieldId);

        } // ReadFieldValues

        public bool FieldExists(string fieldId)
        {
            return _authRepository.CheckFieldExists(fieldId);

        } // FieldExists

        public void DeleteField(string fieldId)
        {
            _authRepository.DeleteField(fieldId);

        } // DeleteField

        #endregion

        #region Authorization Object

        public void CreateAuthorizationObject(string authorizationObjectId, string authorizationGroupId, List<string> fieldList)
        {

            // Check whether all fields are declared

            _authRepository.CheckFieldListExist(fieldList);

            // Create the authorization object 

            _authRepository.CreateAuthorizationObject(authorizationObjectId, authorizationGroupId, fieldList);

        } // CreateAuthorizationObject

        public List<string> ReadAuthorizationObject(string authorizationObjectId)
        {
            return _authRepository.ReadAuthorizationObject(authorizationObjectId);

        } // ReadAuthorizationObject

        public bool AuthorizationObjectExists(string authorizationObjectId)
        {
            return _authRepository.AuthorizationObjectExists(authorizationObjectId);

        } // AuthorizationObjectExists

        public void DeleteAuthorizationObject(string authorizationObjectId)
        {
            _authRepository.DeleteAuthorizationObject(authorizationObjectId);

        } // DeleteAuthorizationObject

        #endregion

        #region Authorization

        public void CreateAuthorization(string authorizationObjectId, Dictionary<string, string> authorizationInstance)
        {
            // Check whether the authorizationInstance matches the authorization object definition

            var authObjectFields = _authRepository.ReadAuthorizationObject(authorizationObjectId);

            authObjectFields.Sort();

            var authFields = authorizationInstance.Select(x => x.Key).ToList();
            authFields.Sort();

            if (!Enumerable.SequenceEqual(authObjectFields, authFields))
                throw new AuthorizationException("The authorization instance does not match with the authorization object definition");

            // Check all fields exist

            foreach (var fieldId in authFields)
                if (!this.FieldExists(fieldId))
                    throw new Exception("Field '" + fieldId + "' definition not found in database!");

            // Check whether all field values are valid values

            foreach (var authRecord in authorizationInstance.Where(x => x.Value != _ALL))
                if (!_authRepository.GetFieldValues(authRecord.Key).Contains(authRecord.Value))
                    throw new AuthorizationException("Value '" + authRecord.Value + "' is not a valid value for field '" + authRecord.Key + "'");

            // Generate the authorization record(s)

            List<string> fieldWithWildcardValuesList = new List<string>();
            fieldWithWildcardValuesList.AddRange(authorizationInstance.Where(x => x.Value == _ALL).Select(x => x.Key));

            List<List<string>> workingList = new List<List<string>>();
            List<List<string>> masterList = new List<List<string>>();

            foreach (var authField in authFields)
                if (fieldWithWildcardValuesList.FirstOrDefault(x => x == authField) != null)
                {
                    // If this auth field contains a wildcard we generate all posible values for the working list
                    workingList.Add(new List<string>(this.ReadFieldValues(authField)));
                    masterList.Add(new List<string>(this.ReadFieldValues(authField)));
                }
                else
                {
                    // On the other hand, if this auth field contains a explicit value we generate a single value list for the working list
                    string value;
                    authorizationInstance.TryGetValue(authField, out value);
                    var singleValueList = new List<string>();
                    singleValueList.Add(value);
                    workingList.Add(new List<string>(singleValueList));
                    masterList.Add(new List<string>(singleValueList));
                }

            List<List<string>> resultList = new List<List<string>>();

            // Generating all matching Authorization Instances from the wildcard values

            //this.GenerateAuthorizationsRecursive(workingList, masterList, resultList);
            Thread thread = new Thread(() => GenerateAuthorizationsRecursive(workingList, masterList, resultList), _RecursiveStackSize);
            thread.Start();
            thread.Join();


            _authRepository.SaveAuthorizationList(authorizationObjectId, authFields, resultList);

        } // CreateAuthorization

        public Dictionary<string, string> GetAuthorizationById(string authorizationId)
        {
            return GetAuthorizationById(new Guid(authorizationId));
        }

        public Dictionary<string, string> GetAuthorizationById(Guid authorizationId)
        {
            return _authRepository.GetAuthorizationById(authorizationId);

        } // GetAuthorirationByHashCode

        public void DeleteAuthorizationById(Guid authorizationId)
        {
            _authRepository.DeleteAuthorizationById(authorizationId);

        } // DeleteAuthorizationByHashCode

        #endregion

        #region User

        public void AssignAuth2User(string userId, string authorizationId, string secret = null)
        {
            // parameter secret != will supose to regenerate all UMR entries for the user when the secret assigned to
            // the given UserId changes.
            AssignAuth2User(userId, new Guid(authorizationId), secret);
        }

        public void AssignAuth2User(string userId, Guid authorizationId, string secret = null)
        {
            _authRepository.AssignAuth2User(userId, authorizationId, secret);

        } // AssignAuth2User

        public void AssignProfile2User(string userId, string profileId, string secret = null)
        {

            var authorizationIdList = _authRepository.GetAuthorizationByProfile(profileId);

            foreach (var authId in authorizationIdList)
            {
                var authorization = this.GetAuthorizationById(authId);
                this.AssignAuth2User(userId, authId, secret);
            }
        } // AssignRole2User

        public HashSet<string> GetUserMasterRecord(string userId)
        {
            return _authRepository.GetUserMasterRecord(userId);

        } // GetUserMasterRecord

        public string GetUserMasterRecordFingerprint(string userId)
        {
            var userMasterRecord = GetUserMasterRecord(userId);

            var authorizationList = userMasterRecord.ToList();

            authorizationList.Sort();

            StringBuilder sb = new StringBuilder();

            foreach (var str in authorizationList)
                sb.Append(str);

            return GetHashSha256(sb.ToString());

        } // GetUserMasterRecordFingerprint

        #endregion

        #region Profile
        public void AssignAuth2Profile(string profileId, List<Guid> authorizationList)
        {

            // Check whether all authorizationIds do exist as valid authorizations in AUTH/Authorizations table
            if (!_authRepository.CheckAuthorizationListIsValid(authorizationList))
                throw new AuthorizationException("Not all authorizationIds were found as valid, please check the authorization list");


            // Insert Authorizations in the profile
            _authRepository.InsertAuthorizationListIntoProfile(profileId, authorizationList);

        } // AssignAuth2Profile

        #endregion

        #endregion

        #region Private Methods

        private void GenerateAuthorizationsRecursive(List<List<string>> workinglist, List<List<string>> masterList, List<List<string>> resultList)
        {
            List<string> result = new List<string>();

            foreach (var subList in workinglist)
                result.Add(subList.First());

            resultList.Add(result);

            // Generate the next recursive call

            for (int i = workinglist.Count - 1; i >= 0; i--)
            {
                workinglist[i].RemoveAt(0);

                if (workinglist[i].Count == 0)
                    workinglist[i].AddRange(masterList[i]);

                else
                {
                    GenerateAuthorizationsRecursive(workinglist, masterList, resultList);
                    break;
                }
            }

        } // GenerateAuthorizationsRecursive

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

    } // class AuthorizationEngine

}
