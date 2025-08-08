using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Models
{
    public class Proxy
    {
        public string Address { get; set; }
        public string Port { get; set; }
        public string? Username { get; set; } = null;
        public string? Password { get; set; } = null;
        public string? Description { get; set; } = null;
        public ProxyProtocol Protocol { get; set; }
        public string FullProxyAddressString
        {
            get
            {
                return $"{Protocol}://{Address}:{Port}";
            }
        }
        public Uri FullProxyAddressUri
        {
            get
            {
                return new Uri($"{Protocol}://{Address}:{Port}");
            }
        }

        public Proxy(string address, string port, ProxyProtocol protocol, string? username = null, string? password = null, string? description = null)
        {
            Address = address;
            Port = port;
            Protocol = protocol;
            Username = username;
            Password = password;
            Description = description;
        }

        public enum ProxyProtocol
        {
            http,
            socks4,
            socks5
        }
    }
}
