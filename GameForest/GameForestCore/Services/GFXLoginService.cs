using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;
using GameForestCore.Common;

namespace GameForestCore.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GFXLoginService : IGFXLoginService
    {
        private readonly List<string>   loggedInUsersTableColumns   = new List<string> { "SessionId", "UserId", "LastHeartbeat" };
        private readonly List<string>   registeredUsersTableColumns = new List<string> { "UserId", "UserName", "Password", "FirstName", "LastName", "Description" };
        
        private readonly SqlConnection  connection;

        public  GFXLoginService                     ()
        {
            var webConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);

            if (webConfig.ConnectionStrings.ConnectionStrings.Count == 0)
                return;

            var connSetting = webConfig.ConnectionStrings.ConnectionStrings["GameForestConnection"];

            if (connSetting == null)
                return;

            this.connection = new SqlConnection(connSetting.ConnectionString);
            this.connection.Open();
        }

        public  GFXRestResponse Login               (string username, string password)
        {
            if (!usernameExists(username))
            {
                return constructResponse(GFXResponseType.NotFound, "Username does not exist!");
            }

            if (!passwordMatch(username, password))
            {
                return constructResponse(GFXResponseType.InvalidInput, "Passwords does not match!");
            }

            

            return null;
        }

        public  GFXRestResponse Logout              (string usersessionid)
        {
            return null;
        }

        public  GFXRestResponse Heartbeat           (string usersessionid)
        {
            return null;
        }

        public GFXRestResponse  UpdateInfo          (string usersessionid, string firstname, string lastname, string description)
        {
            return null;
        }

        public GFXRestResponse  UpdatePword         (string usersessionid, string newpassword, string curpassword)
        {
            return null;
        }

        public  GFXRestResponse Register            (string username, string password)
        {
            if (connection != null)
            {
                // error checking
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return constructResponse(GFXResponseType.InvalidInput, "Username or password is left blank.");
                }

                if (usernameExists(username))
                {
                    return constructResponse(GFXResponseType.DuplicateEntry, "Username already exists!");
                }

                // add user to database
                try
                {
                    var valueList = new List<object> { Guid.NewGuid().ToString(), username, password, "", "", "New GameForest user." };

                    GFXDatabaseHelper.Insert(connection, "RegisteredUsers", registeredUsersTableColumns, valueList);

                    return constructResponse(GFXResponseType.Normal, "Okay.");
                }
                catch (Exception exp)
                {
                    return constructResponse(GFXResponseType.RuntimeError, "Error is SQL execution. " + exp.Message);
                }
            }

            return constructResponse(GFXResponseType.FatalError, "SQL server is not running or service is not connected to database.");
        }

        public  GFXRestResponse Unregister          (string usersessionid, string password)
        {
            return null;
        }

        // ----------------------------------------------------------------------------------------------------------------

        private bool            usernameExists      (string username)
        {
            var query = new SqlCommand(string.Format("SELECT COUNT(userid) FROM RegisteredUsers WHERE username = '{0}'", username));

            return (int)query.ExecuteScalar() == 1;
        }

        private bool            passwordMatch       (string username, string password)
        {
            var query   = new SqlCommand(string.Format("SELECT password FROM RegisteredUsers WHERE username = '{0}'", username));
            
            return (string)query.ExecuteScalar() == password;
        }

        private Guid            getUserId           (string username)
        {
            var query = new SqlCommand(string.Format("SELECT userid FROM RegisteredUsers WHERE username = '{0}'", username));

            return (Guid)query.ExecuteScalar();
        }

        private GFXRestResponse constructResponse   (GFXResponseType responseType, string payload)
        {
            return new GFXRestResponse { AdditionalData = payload, ResponseType = responseType };
        }
    }
}
