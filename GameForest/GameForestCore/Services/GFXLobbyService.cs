using GameForestCore.Common;
using GameForestCore.Database;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace GameForestCore.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single), AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class GFXLobbyService : IGFXLobbyService
    {
        private readonly GFXDatabaseTable<GFXUserRow>   userTable;
        private readonly GFXDatabaseTable<GFXLoginRow>  loginTable; 

        public GFXLobbyService()
        {

        }

        public GFXLobbyService(string connectionString)
        {

        }

        public GFXRestResponse GetLobbies(int maxcount)
        {
            throw new NotImplementedException();
        }

        public GFXRestResponse GetLobby(string lobbyid)
        {
            throw new NotImplementedException();
        }

        public GFXRestResponse CreateLobby(string lobbyname, string gameid, string password)
        {
            throw new NotImplementedException();
        }

        public GFXRestResponse JoinLobby(string lobbyid, string usersessionid)
        {
            throw new NotImplementedException();
        }

        public GFXRestResponse LeaveLobby(string usersessionid)
        {
            throw new NotImplementedException();
        }

        public GFXRestResponse KickLobby(string usersessionid, string usertokick)
        {
            throw new NotImplementedException();
        }
    }
}
