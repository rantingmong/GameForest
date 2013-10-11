using GameForestCore.Common;
using GameForestCore.Database;
using GameForestDatabaseConnector.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GameForestCore.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GFXUserService : IGFXUserService
    {
        private readonly GFXDatabaseTable<GFXUserRow>       userTable;
        private readonly GFXDatabaseTable<GFXLoginRow>      loginTable;

        public                      GFXUserService          ()
        {
            userTable = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());
            loginTable = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());
        }

        // ----------------------------------------------------------------------------------------------------------------

        public  GFXRestResponse     GetUserInfo             (string username)
        {
            if (!userExists(username))
                return constructResponse(GFXResponseType.NotFound, "Username or user id cannot be found.");

            try
            {
                Guid    userId      = Guid.Empty;
                var     results     = new List<GFXUserRow>();

                if (Guid.TryParse(username, out userId))
                {
                    results.AddRange(userTable.Select(string.Format("userid = '{0}'", userId)));
                }
                else
                {
                    results.AddRange(userTable.Select(string.Format("username = '{0}'", username)));
                }
                
                return constructResponse(GFXResponseType.Normal, userInfoToJson(results[0]));
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|GetUserInfo] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse     GetUserInfoFromSession  (string usersessionid)
        {
            if (!sessionExists(usersessionid))
                return constructResponse(GFXResponseType.NotFound, "Session cannot be found.");

            try
            {// get user id from session id
                var session = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", usersessionid)));
                var results = new List<GFXUserRow>(userTable.Select(string.Format("UserId = '{0}'", session[0].UserId)));

                return constructResponse(GFXResponseType.Normal, userInfoToJson(results[0]));
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|GetUserInfoFromSession] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse     SetUserInfo             (string username, string firstname, string lastname, string description, string usersessionid)
        {
            if (!sessionExists(usersessionid))
                return constructResponse(GFXResponseType.NotFound, "User not logged in.");

            if (!userExists(username))
                return constructResponse(GFXResponseType.NotFound, "Username or user id cannot be found.");

            try
            {
                // get the user's data, then update.
                Guid userId;

                if (Guid.TryParse(username, out userId))
                {
                    var result = new List<GFXUserRow>(userTable.Select(string.Format("userid = '{0}'", userId)))[0];

                    result.FirstName = firstname;
                    result.LastName = lastname;
                    result.Description = description;
                    result.Username = username;

                    userTable.Update(string.Format("userid = '{0}'", userId), result);
                }
                else
                {
                    var result = new List<GFXUserRow>(userTable.Select(string.Format("username = '{0}'", username)))[0];

                    result.FirstName = firstname;
                    result.LastName = lastname;
                    result.Description = description;

                    userTable.Update(string.Format("username = '{0}'", username), result);
                }
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|SetUserInfo] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }

            return constructResponse(GFXResponseType.Normal, "Success!");
        }

        public  GFXRestResponse     Register                (string username, string password)
        {
            if (userExists(username))
                return constructResponse(GFXResponseType.DuplicateEntry, "Username already exists!");

            if (password.Length < 5 || password.Length > 20)
                return constructResponse(GFXResponseType.InvalidInput, "Invalid password length!");

            if (username.Length < 5 || username.Length > 40)
                return constructResponse(GFXResponseType.InvalidInput, "Invalid username length!");

            try
            {
                userTable.Insert(new GFXUserRow
                {
                    Description = "",
                    FirstName = "GameForest",
                    LastName = "User",
                    Username = username,
                    Password = password,
                    UserId = Guid.NewGuid()
                });

                return constructResponse(GFXResponseType.Normal, "Success!");
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|Register] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse     Unregister              (string username, string password)
        {
            if (!userExists(username))
                return constructResponse(GFXResponseType.NotFound, "Username not found!");

            try
            {
                if (!passwordMatch(username, password))
                {
                    return constructResponse(GFXResponseType.InvalidInput, "Passwords does not match!");
                }

                Guid userId;
                this.userTable.Remove(Guid.TryParse(username, out userId) ? string.Format("userid = '{0}'", userId) : string.Format("username = '{0}'", username));

                return constructResponse(GFXResponseType.Normal, "Success!");
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|Unregister] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse     Login                   (string username, string password)
        {
            if (!userExists(username))
                return constructResponse(GFXResponseType.NotFound, "Username or user id not found!");

            if (!passwordMatch(username, password))
                return constructResponse(GFXResponseType.InvalidInput, "Passwords does not match!");

            try
            {
                var sessionId = Guid.NewGuid();
                var userId = getUserId(username);

                if (userId == Guid.Empty)
                    return constructResponse(GFXResponseType.RuntimeError, "Error in finding user id.");

                loginTable.Insert(new GFXLoginRow
                {
                    LastHeartbeat   = DateTime.Now,
                    UserId          = userId,
                    SessionId       = sessionId,
                    UserStatus      = GFXLoginStatus.MENU,
                });

                return constructResponse(GFXResponseType.Normal, sessionId.ToString());
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|Login] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse     Logout                  (string usersessionid)
        {
            if (!sessionExists(usersessionid))
                return constructResponse(GFXResponseType.NotFound, "User session not found!");

            try
            {
                loginTable.Remove(string.Format("sessionid = '{0}'", usersessionid));

                return constructResponse(GFXResponseType.Normal, "Success");
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|Logout] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse     Heartbeat               (string usersessionid)
        {
            if (!sessionExists(usersessionid))
                return constructResponse(GFXResponseType.NotFound, "User session not found!");

            try
            {
                var result = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", usersessionid)));

                if (result.Count <= 0)
                    return constructResponse(GFXResponseType.NotFound, "Session expired.");

                var user = result[0];

                user.LastHeartbeat = DateTime.Now;

                loginTable.Update(string.Format("sessionid = '{0}'", usersessionid), user);

                return constructResponse(GFXResponseType.Normal, "Success");
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|Heartbeat] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse     ChangePassword          (string usersessionid, string oldpassword, string newpassword)
        {
            if (!sessionExists(usersessionid))
                return constructResponse(GFXResponseType.NotFound, "User session not found!");

            if (oldpassword == newpassword)
            {
                return constructResponse(GFXResponseType.DuplicateEntry, "Passwords match! It should be different.");
            }

            if (newpassword.Length < 5 || newpassword.Length > 20)
                return constructResponse(GFXResponseType.InvalidInput, "Invalid password length!");

            try
            {
                var userid = getUserId(usersessionid);
                var result = new List<GFXUserRow>(userTable.Select(string.Format("userid = '{0}'", userid)))[0];

                result.Password = newpassword;

                userTable.Update(string.Format("userid = '{0}'", userid), result);

                return constructResponse(GFXResponseType.Normal, "Success");
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[User|ChangePassword] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse FBConnect(string fbid, string email, string fname, string lname)
        {
            if (!userExists(email))
            {
                try
                {
                    var sessionId = Guid.NewGuid();

                    userTable.Insert(new GFXUserRow
                    {
                        Description = "",
                        FirstName = fname,
                        LastName = lname,
                        Username = email,
                        Password = "debugger",
                        UserId = Guid.NewGuid(),
                        fb_id = fbid
                    });

                    return constructResponse(GFXResponseType.Normal, sessionId.ToString());
                }
                catch (Exception exp)
                {
                    Console.Error.WriteLine("[User|FBConn] " + exp.Message);

                    return constructResponse(GFXResponseType.RuntimeError, exp.Message);
                }
            }

            if (userExists(email))
            {
                try
                {
                    var sessionId = Guid.NewGuid();
                    var userId = getUserId(email);

                    if (userId == Guid.Empty)
                        return constructResponse(GFXResponseType.RuntimeError, "Error in finding user id");

                    if (loginTable.Count(string.Format("UserId = '{0}'", userId)) == 0)
                    {
                        loginTable.Insert(new GFXLoginRow
                        {
                            LastHeartbeat = DateTime.Now,
                            UserId = userId,
                            SessionId = sessionId,
                            UserStatus = GFXLoginStatus.MENU
                        });

                        return constructResponse(GFXResponseType.Normal, sessionId.ToString());
                    }
                    else
                    {
                        var result = new List<GFXLoginRow>(loginTable.Select(string.Format("UserId = '{0}'", userId)));

                        if (result.Count <= 0)
                            return constructResponse(GFXResponseType.NotFound, "Session expired");

                        var user = result[0];

                        user.LastHeartbeat = DateTime.Now;

                        loginTable.Update(string.Format("UserId = '{0}'", userId), user);

                        sessionId = user.SessionId;

                        return constructResponse(GFXResponseType.Normal, sessionId.ToString());
                    }
                }
                catch (Exception exp)
                {
                    Console.Error.WriteLine("[User|FBConn] " + exp.Message);

                    return constructResponse(GFXResponseType.RuntimeError, exp.Message);
                }
            }

            return constructResponse(GFXResponseType.Normal, "[User|FBConn] Something went wrong");
        }

        // ----------------------------------------------------------------------------------------------------------------

        private string              userInfoToJson          (GFXUserRow userRow)
        {
            var uResult = new Dictionary<string, object>();

            uResult["Name"]         = userRow.FirstName + " " + userRow.LastName;
            uResult["UserId"]       = userRow.UserId;
            uResult["Description"]  = userRow.Description;
            uResult["Username"]     = userRow.Username;

            return JsonConvert.SerializeObject(uResult);
        }

        private bool                userExists              (string user)
        {
            Guid userId;

            if (Guid.TryParse(user, out userId))
                return userTable.Count(string.Format("userid = '{0}'", userId)) == 1;

            return this.userTable.Count(string.Format("username = '{0}'", user)) == 1;
        }

        private bool                sessionExists           (string sessionid)
        {
            return loginTable.Count(string.Format("sessionid = '{0}'", sessionid)) == 1;
        }

        private bool                passwordMatch           (string username, string password)
        {
            var asd = new List<GFXUserRow>(userTable.Select(string.Format("username = '{0}'", username)));

            if (asd.Count > 0)
            {
                return asd[0].Password == password;
            }

            return false;
        }

        private Guid                getUserId               (string input)
        {
            Guid sessionId;

            if (Guid.TryParse(input, out sessionId))
            {
                var result = new List<GFXLoginRow>(loginTable.Select(string.Format("sessionid = '{0}'", sessionId)));

                return result.Count > 0 ? result[0].UserId : Guid.Empty;
            }
            else
            {
                var result = new List<GFXUserRow>(userTable.Select(string.Format("username = '{0}'", input)));

                return result.Count > 0 ? result[0].UserId : Guid.Empty;
            }
        }

        private GFXRestResponse     constructResponse       (GFXResponseType responseType, string payload)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "constructResponse", "Returning result with type" + responseType + " and payload " + payload);

            return new GFXRestResponse { AdditionalData = payload, ResponseType = responseType };
        }
    }
}
