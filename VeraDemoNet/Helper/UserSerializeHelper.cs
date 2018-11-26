using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using VeraDemoNet.DataAccess;
using VeraDemoNet.Models;

namespace VeraDemoNet.Helper
{
    public class UserSerializeHelper
    {
        private const string COOKIE_NAME = "UserDetails";

        public static CustomSerializeModel CreateFromRequest(HttpRequestBase request, log4net.ILog logger)
        {
            var serializedUserDetails = request.Cookies[COOKIE_NAME];

            if (serializedUserDetails == null)
            {
                logger.Info("No user cookie");
                return null;
            }

            if (serializedUserDetails.Value.Length == 0)
            {
                logger.Info("User cookie is empty");
                return null;
            }


            logger.Info("User details were remembered");
            var unencodedUserDetails = Convert.FromBase64String(serializedUserDetails.Value);

            using (MemoryStream memoryStream = new MemoryStream(unencodedUserDetails))
            {
                var binaryFormatter = new BinaryFormatter();

                // set memory stream position to starting point
                memoryStream.Position = 0;

                // Deserializes a stream into an object graph and return as a object.
                /* START BAD CODE */
                var deserializedUser = binaryFormatter.Deserialize(memoryStream) as CustomSerializeModel;
                /* END BAD CODE */
                logger.Info("User details were retrieved for user: " + deserializedUser.UserName);
                return deserializedUser;
            }
        }

        public static void UpdateResponse(HttpResponseBase response, log4net.ILog logger, CustomSerializeModel userModel)
        {
            using (var userModelStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(userModelStream, userModel);
                var faCookie = new HttpCookie("UserDetails", Convert.ToBase64String(userModelStream.GetBuffer()));
                faCookie.Expires = DateTime.Now.AddDays(30);
                response.Cookies.Add(faCookie);
            }
        }

    }
}