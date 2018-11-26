using System;

namespace VeraDemoNet.Models
{
    [Serializable]
    public class CustomSerializeModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string BlabName { get; set; }
        public string RealName { get; set; }
    }
}