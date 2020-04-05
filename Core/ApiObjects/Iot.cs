using ApiObjects.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class Iot : IotRegisterResponse, ICrudHandeledObject
    {
        public string GroupId { get; set; }
        public string Udid { get; set; }
    }

    public class IotRegisterResponse
    {
        //Cognito
        public string AccessKey { get; set; }
        public string AccessSecretKey { get; set; }
        public string Username { get; set; }
        public string UserPassword { get; set; }
        public string IdentityId { get; set; }

        //Iot
        public string ThingArn { get; set; }
        public string ThingId { get; set; }
        public string Principal { get; set; }
        public string EndPoint { get; set; }
        public string ExtendedEndPoint { get; set; }

        //env
        public string IdentityPoolId { get; set; }
        public string UserPoolId { get; set; }
        public string UserPoolWebClientId { get; set; }
    }

    public class IotClientConfiguration
    {
        public CredentialsProvider CredentialsProvider { get; set; }
        public CognitoUserPool CognitoUserPool { get; set; }
        public string AnnouncementTopic { get; set; }
        public string Json { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(new { this.CredentialsProvider, this.CognitoUserPool }, Formatting.None).Replace("IotDefault", "Default");
        }
    }

    public class CredentialsProvider
    {
        public CognitoIdentity CognitoIdentity { get; set; }
    }

    public class CognitoIdentity
    {
        public IotDefault IotDefault { get; set; }
    }

    public class CognitoUserPool
    {
        public IotDefault IotDefault { get; set; }
    }

    public class IotDefault
    {
        public string PoolId { get; set; }
        public string Region { get; set; }
        public string AppClientId { get; set; }
    }
}
