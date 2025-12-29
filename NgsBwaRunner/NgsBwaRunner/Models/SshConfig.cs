namespace NgsBwaRunner.Models
{
    public class SshConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public SshConfig()
        {
            Host = Config.Settings.DEFAULT_HOST;
            Port = Config.Settings.DEFAULT_PORT;
            Username = Config.Settings.DEFAULT_USER;
            Password = Config.Settings.DEFAULT_PASS;
        }

        public SshConfig(string host, int port, string user, string pass)
        {
            Host = host;
            Port = port;
            Username = user;
            Password = pass;
        }

        // 验证配置是否有效
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Host) &&
                   !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) &&
                   Port > 0 && Port <= 65535;
        }
    }
}