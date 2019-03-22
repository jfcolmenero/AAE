using System;
using System.Collections.Generic;
using System.Text;

namespace AAE.Contracts
{
    public interface IAuthorizationRepository
    {
        #region AuthorizationGroup

        void CreateAuthorizationGroup(string authorizationGroupId);

        bool AuthorizationGroupExists(string authorizationGroupId);

        void DeleteAuthorizationGroup(string authorizationGroupId);

        #endregion

        #region Fields

        void CreateField(string fieldId, List<string> valueList);

        void UpdateFieldValues(string fieldId, List<string> valueList);

        bool CheckFieldExists(string fieldId);

        void CheckFieldListExist(List<string> fieldList);

        List<string> GetFieldValues(string fieldId);

        void DeleteField(string fieldId);


        #endregion

        #region AUthorization Object

        void CreateAuthorizationObject(string authorizationObjectId, string authorizationGroupId, List<string> fieldList);
        List<string> ReadAuthorizationObject(string authorizationObjectId);
        bool AuthorizationObjectExists(string authorizationObjectId);
        void DeleteAuthorizationObject(string authorizationObjectId);

        #endregion

        #region Authorizations

        void SaveAuthorizationList(string authorizationObjectId, List<string> authFields, List<List<string>> resultList);
        Dictionary<string, string> GetAuthorizationById(Guid authorizationId);
        void DeleteAuthorizationById(Guid authorizationId);

        #endregion

        #region User

        void AssignAuth2User(string userId, Guid authorizationId, string secret);

        HashSet<string> GetUserMasterRecord(string userId);

        #endregion

        #region Profile

        bool CheckAuthorizationListIsValid(List<Guid> authorizationList);
        void InsertAuthorizationListIntoProfile(string profileId, List<Guid> authorizationList);
        List<Guid> GetAuthorizationByProfile(string profileId);

        #endregion

    }

}
