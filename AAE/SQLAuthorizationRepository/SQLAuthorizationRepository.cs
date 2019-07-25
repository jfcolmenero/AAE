using AAE;
using AAE.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SQLAuthorizationRepository
{
    public class SQLAuthorizationRepository : IAuthorizationRepository
    {
        private string _connectionString;

        public SQLAuthorizationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        #region Authorization Group

        public bool AuthorizationGroupExists(string authorizationGroupId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authGroup = db.AUTH_AuthorizationGroups.FirstOrDefault(x => x.AuthorizationObjectGroupId == authorizationGroupId);

                if (authGroup != null)
                    return authGroup.AuthorizationObjectGroupId == authorizationGroupId;
                else
                    return false;
            }
        }

        public void CreateAuthorizationGroup(string authorizationGroupId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                AUTH_AuthorizationGroup authorizationGroup = new AUTH_AuthorizationGroup()
                {
                    AuthorizationObjectGroupId = authorizationGroupId
                };

                try
                {
                    db.AUTH_AuthorizationGroups.InsertOnSubmit(authorizationGroup);

                    db.SubmitChanges();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                        throw new AuthorizationException("Authorization Group '" + authorizationGroupId + "' already exists.");
                }

            } // using
        }

        public void DeleteAuthorizationGroup(string authorizationGroupId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authGroup = db.AUTH_AuthorizationGroups.FirstOrDefault(x => x.AuthorizationObjectGroupId == authorizationGroupId);

                if (authGroup != null)
                {
                    db.AUTH_AuthorizationGroups.DeleteOnSubmit(authGroup);

                    db.SubmitChanges();
                }
                else
                    throw new AuthorizationException("Authorization Group '" + authorizationGroupId + "' not found.");
            }
        }

        #endregion

        #region Field

        public void CreateField(string fieldId, List<string> valueList)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                AUTH_Field field = new AUTH_Field()
                {
                    FieldId = fieldId,
                    FieldValues = JsonConvert.SerializeObject(valueList)
                };

                try
                {
                    db.AUTH_Fields.InsertOnSubmit(field);

                    db.SubmitChanges();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                        throw new AuthorizationException("Field Object '" + fieldId + "' already exists.");
                }
            }
        }

        public void UpdateFieldValues(string fieldId, List<string> valueList)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var field = db.AUTH_Fields.First(x => x.FieldId == fieldId);

                field.FieldValues = JsonConvert.SerializeObject(valueList);

                db.SubmitChanges();
            }
        }

        public List<string> GetFieldValues(string fieldId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var field = db.AUTH_Fields.First(x => x.FieldId == fieldId);

                var appendList = this.GetList(field.FieldValues);

                return appendList;
            }
        }

        public bool CheckFieldExists(string fieldId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var field = db.AUTH_Fields.SingleOrDefault(x => x.FieldId == fieldId);

                return (field != null);
            }
        }

        public void CheckFieldListExist(List<string> fieldList)
        {
            if (fieldList.Count() == 0)
                throw new AuthorizationException("Field list cannot be empty.");

            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                foreach (var field in fieldList)
                    if (db.AUTH_Fields.FirstOrDefault(x => x.FieldId == field) == null)
                        throw new AuthorizationException("FieldId: '" + field + "' has not been declared as a valid FieldId.");
            }
        }

        public void DeleteField(string fieldId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var field = db.AUTH_Fields.First(x => x.FieldId == fieldId);

                db.AUTH_Fields.DeleteOnSubmit(field);

                db.SubmitChanges();
            }
        }

        #endregion

        #region Authorization Object

        public void CreateAuthorizationObject(string authorizationObjectId, string authorizationGroupId, List<string> fieldList)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                AUTH_AuthorizationObject authorizationObject = new AUTH_AuthorizationObject()
                {
                    AuthorizationObjectGroupId = authorizationGroupId,
                    FieldList = JsonConvert.SerializeObject(fieldList),
                    AuthorizationObjectId = authorizationObjectId
                };

                try
                {
                    db.AUTH_AuthorizationObjects.InsertOnSubmit(authorizationObject);

                    db.SubmitChanges();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                        throw new AuthorizationException("Authorization Object '" + authorizationObjectId + "' already exists.");
                    else if (ex.Number == 547)
                        throw new AuthorizationException("Authorization Group '" + authorizationGroupId + "' does not exist.");
                    else
                        throw ex;
                }
            }
        }

        public List<string> ReadAuthorizationObject(string authorizationObjectId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authorizationObject = db.AUTH_AuthorizationObjects.Single(x => x.AuthorizationObjectId == authorizationObjectId);

                return this.GetList(authorizationObject.FieldList);
            }
        }

        public bool AuthorizationObjectExists(string authorizationObjectId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authObject = db.AUTH_AuthorizationObjects.SingleOrDefault(x => x.AuthorizationObjectId == authorizationObjectId);

                return (authObject != null);
            }
        }

        public void DeleteAuthorizationObject(string authorizationObjectId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authObject = db.AUTH_AuthorizationObjects.First(x => x.AuthorizationObjectId == authorizationObjectId);

                db.AUTH_AuthorizationObjects.DeleteOnSubmit(authObject);

                db.SubmitChanges();
            }
        }

        #endregion

        #region Authorization

        public void SaveAuthorizationList(string authorizationObjectId, List<string> authFields, List<List<string>> resultList)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                // After recursive generations of all posible values we generate the authorization instances in database
                Dictionary<string, string> authorizationInstanceAux = new Dictionary<string, string>();
                List<AUTH_Authorization> authorizationInstancesList = new List<AUTH_Authorization>();
                string jsonOutput;
                foreach (var resultSet in resultList)
                {
                    authorizationInstanceAux.Clear();
                    int i = 0;
                    foreach (var authField in authFields)
                        authorizationInstanceAux.Add(authField, resultSet[i++]);

                    jsonOutput = JsonConvert.SerializeObject(authorizationInstanceAux);

                    AUTH_Authorization authorization = new AUTH_Authorization()
                    {
                        AuthorizationObjectId = authorizationObjectId,
                        ValueList = jsonOutput,
                        //HashCode = GetHashSha256(authorizationObjectId + jsonOutput),
                        AuthorizationId = Guid.NewGuid(),
                        Hash = GetHashSha256(jsonOutput)
                    };

                    authorizationInstancesList.Add(authorization);
                    //db.AUTH_Authorizations.InsertOnSubmit(authorization);
                }

                try
                {
                    db.AUTH_Authorizations.InsertAllOnSubmit(authorizationInstancesList);
                    db.SubmitChanges();
                }
                catch (SqlException ex)
                {
                    var conflict = ex.Message.Split('(')[1].Split(')')[0];

                    if (ex.Number == 2601)
                        throw new AuthorizationException("Authorization with hascode '" + conflict + "' already exists. 0 of " + authorizationInstancesList.Count + " authorizations were created.");
                    else
                        throw ex;
                }
            }
        }

        public Dictionary<string, string> GetAuthorizationById(Guid authorizationId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authorization = db.AUTH_Authorizations.FirstOrDefault(x => x.AuthorizationId == authorizationId);

                if (authorization != null)

                    return this.GetDictionary(authorization.ValueList);
                else
                    return null;
            }
        }

        public void DeleteAuthorizationById(Guid authorizationId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authorization = db.AUTH_Authorizations.Single(x => x.AuthorizationId == authorizationId);

                db.AUTH_Authorizations.DeleteOnSubmit(authorization);

                db.SubmitChanges();
            }
        }

        #endregion

        #region User

        public void AssignAuth2User(string userId, Guid authorizationId, string secret)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var auth = db.AUTH_Authorizations.Single(x => x.AuthorizationId == authorizationId);

                AUTH_UsersMasterRecord userAuth = new AUTH_UsersMasterRecord()
                {
                    UserId = userId,
                    HashCode = GetHashSha256(userId + auth.AuthorizationObjectId + auth.ValueList + secret),
                };

                try
                {
                    db.AUTH_UsersMasterRecords.InsertOnSubmit(userAuth);

                    db.SubmitChanges();
                }
                catch (SqlException ex)
                {

                    if (ex.Number == 2627)
                        throw new AuthorizationException("AuthorizationId '" + authorizationId + "' was already assigned to user '" + userId + ".");
                    else
                        throw ex;
                }

            } // using
        }

        public HashSet<string> GetUserMasterRecord(string userId)
        {
            HashSet<string> userMasterRecord;

            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                userMasterRecord = new HashSet<string>(db.AUTH_UsersMasterRecords.Where(x => x.UserId == userId).Select(x => x.HashCode));

            } // using

            return userMasterRecord;
        }

        #endregion

        #region Profile

        public bool CheckAuthorizationListIsValid(List<Guid> authorizationList)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                int offset = 0, batchSize = 1000, dbAuthorizationListCount = 0;

                int batchCount = (from auth in db.AUTH_Authorizations
                                  where authorizationList.Skip(offset).Take(batchSize).Contains(auth.AuthorizationId)
                                  select auth).Count();
                while (batchCount > 0)
                {
                    dbAuthorizationListCount += batchCount;

                    offset += batchSize;

                    batchCount = (from auth in db.AUTH_Authorizations
                                  where authorizationList.Skip(offset).Take(batchSize).Contains(auth.AuthorizationId)
                                  select auth).Count();
                }

                return (dbAuthorizationListCount != authorizationList.Count);
            }
        }

        public void InsertAuthorizationListIntoProfile(string profileId, List<Guid> authorizationList)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                foreach (var authId in authorizationList)
                {
                    AUTH_ProfilesContent profileContent = new AUTH_ProfilesContent()
                    {
                        ProfileId = profileId,
                        AuthorizationId = authId

                    };

                    db.AUTH_ProfilesContents.InsertOnSubmit(profileContent);
                }

                db.SubmitChanges();
            }
        }

        public List<Guid> GetAuthorizationByProfile(string profileId)
        {
            using (AUTHDBDataContext db = new AUTHDBDataContext(_connectionString))
            {
                var authorizationIdsList = db.AUTH_ProfilesContents.Where(x => x.ProfileId == profileId).Select(x => x.AuthorizationId);

                return authorizationIdsList.ToList();
            }
        }

        #endregion

        #region Private Methods

        private string GetHashSha256(string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            var base64String = System.Convert.ToBase64String(hash);

            return base64String;

        } // GetHashSha256

        private List<string> GetList(string jsonSerializedList)
        {
            var result = JsonConvert.DeserializeObject<List<string>>(jsonSerializedList);

            return result;

        } // GetList

        private Dictionary<string, string> GetDictionary(string jsonSerializedList)
        {
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSerializedList);

            return result;

        } // GetDictionary        

        #endregion

    } // AuthorityRepository

}
