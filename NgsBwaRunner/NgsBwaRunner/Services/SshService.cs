using System;
using System.IO;
using Renci.SshNet;

namespace NgsBwaRunner.Services
{
    public class SshService : IDisposable
    {
        private SshClient? _sshClient;
        private SftpClient? _sftpClient;
        private bool _isConnected;

        public bool IsConnected => _isConnected;

        public SshService() { }

        // 连接到服务器
        public bool Connect(Models.SshConfig config)
        {
            try
            {
                Utils.Logger.LogInfo($"正在连接到服务器 {config.Username}@{config.Host}:{config.Port}");

                _sshClient = new SshClient(config.Host, config.Port, config.Username, config.Password);
                _sshClient.Connect();

                _sftpClient = new SftpClient(config.Host, config.Port, config.Username, config.Password);
                _sftpClient.Connect();

                _isConnected = (_sshClient != null && _sshClient.IsConnected) &&
                              (_sftpClient != null && _sftpClient.IsConnected);

                if (_isConnected)
                {
                    Utils.Logger.LogSuccess("SSH连接成功");
                }
                else
                {
                    Utils.Logger.LogError("SSH连接失败");
                }

                return _isConnected;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"连接服务器失败: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        // 执行命令
        public string ExecuteCommand(string command)
        {
            if (!_isConnected || _sshClient == null)
            {
                Utils.Logger.LogError("SSH未连接，无法执行命令");
                throw new Exception("SSH未连接");
            }

            try
            {
                Utils.Logger.LogInfo($"执行命令: {command}");

                using var cmd = _sshClient.CreateCommand(command);
                cmd.CommandTimeout = TimeSpan.FromSeconds(Config.Settings.COMMAND_TIMEOUT);

                string result = cmd.Execute();

                if (cmd.ExitStatus != 0)
                {
                    string error = cmd.Error;
                    Utils.Logger.LogError($"命令执行失败: {error}");
                    throw new Exception($"命令执行失败: {error}");
                }

                Utils.Logger.LogSuccess("命令执行成功");
                return result;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"执行命令时出错: {ex.Message}");
                throw;
            }
        }

        // 上传文件到服务器
        public bool UploadFile(string localPath, string remotePath)
        {
            try
            {
                if (!_isConnected || _sftpClient == null)
                {
                    Utils.Logger.LogError("SSH未连接，无法上传文件");
                    return false;
                }

                if (!File.Exists(localPath))
                {
                    Utils.Logger.LogError($"本地文件不存在: {localPath}");
                    return false;
                }

                Utils.Logger.LogInfo($"上传文件: {localPath} -> {remotePath}");

                // 确保远程目录存在
                string? remoteDir = Path.GetDirectoryName(remotePath);
                if (!string.IsNullOrEmpty(remoteDir))
                {
                    CreateRemoteDirectory(remoteDir);
                }

                using (var fileStream = File.OpenRead(localPath))
                {
                    _sftpClient.UploadFile(fileStream, remotePath, true);
                }

                // 验证文件上传成功
                if (_sftpClient.Exists(remotePath))
                {
                    var fileInfo = _sftpClient.GetAttributes(remotePath);
                    Utils.Logger.LogSuccess($"文件上传成功，大小: {Utils.FileHelper.FormatFileSize(fileInfo.Size)}");
                    return true;
                }
                else
                {
                    Utils.Logger.LogError("文件上传失败，远程文件不存在");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"上传文件失败: {ex.Message}");
                return false;
            }
        }

        // 从服务器下载文件
        public bool DownloadFile(string remotePath, string localPath)
        {
            try
            {
                if (!_isConnected || _sftpClient == null)
                {
                    Utils.Logger.LogError("SSH未连接，无法下载文件");
                    return false;
                }

                if (!_sftpClient.Exists(remotePath))
                {
                    Utils.Logger.LogError($"远程文件不存在: {remotePath}");
                    return false;
                }

                Utils.Logger.LogInfo($"下载文件: {remotePath} -> {localPath}");

                // 确保本地目录存在
                string? localDir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(localDir))
                {
                    Directory.CreateDirectory(localDir);
                }

                using (var fileStream = File.Create(localPath))
                {
                    _sftpClient.DownloadFile(remotePath, fileStream);
                }

                // 验证文件下载成功
                if (File.Exists(localPath))
                {
                    var fileInfo = new FileInfo(localPath);
                    Utils.Logger.LogSuccess($"文件下载成功，大小: {Utils.FileHelper.FormatFileSize(fileInfo.Length)}");
                    return true;
                }
                else
                {
                    Utils.Logger.LogError("文件下载失败，本地文件不存在");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"下载文件失败: {ex.Message}");
                return false;
            }
        }

        // 创建远程目录
        private void CreateRemoteDirectory(string remoteDir)
        {
            try
            {
                if (!_sftpClient!.Exists(remoteDir))
                {
                    _sftpClient.CreateDirectory(remoteDir);
                    Utils.Logger.LogDebug($"创建远程目录: {remoteDir}");
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogWarning($"创建远程目录失败: {ex.Message}");
            }
        }

        // 检查远程文件是否存在
        public bool RemoteFileExists(string remotePath)
        {
            try
            {
                if (!_isConnected || _sftpClient == null)
                    return false;

                return _sftpClient.Exists(remotePath);
            }
            catch
            {
                return false;
            }
        }

        // 获取远程文件大小
        public long GetRemoteFileSize(string remotePath)
        {
            try
            {
                if (!_isConnected || _sftpClient == null)
                    return 0;

                if (_sftpClient.Exists(remotePath))
                {
                    var attributes = _sftpClient.GetAttributes(remotePath);
                    return attributes.Size;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // 断开连接
        public void Disconnect()
        {
            try
            {
                _sftpClient?.Disconnect();
                _sshClient?.Disconnect();
                _isConnected = false;
                Utils.Logger.LogInfo("SSH连接已断开");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogWarning($"断开连接时出错: {ex.Message}");
            }
        }
        // SshService.cs - 在Dispose()方法前添加以下方法

        /// <summary>
        /// 检查远程目录是否存在
        /// </summary>
        public bool RemoteDirectoryExists(string remotePath)
        {
            try
            {
                if (!_isConnected || _sftpClient == null)
                    return false;

                return _sftpClient.Exists(remotePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除远程目录（递归删除）
        /// </summary>
        public bool DeleteRemoteDirectory(string remotePath)
        {
            try
            {
                if (!_isConnected || _sftpClient == null)
                {
                    Utils.Logger.LogError("SSH未连接，无法删除远程目录");
                    return false;
                }

                if (!RemoteDirectoryExists(remotePath))
                {
                    Utils.Logger.LogInfo($"远程目录不存在: {remotePath}");
                    return true; // 目录不存在也算删除成功
                }

                // 使用SFTP递归删除
                var files = _sftpClient.ListDirectory(remotePath);
                foreach (var file in files)
                {
                    if (file.Name != "." && file.Name != "..")
                    {
                        if (file.IsDirectory)
                        {
                            DeleteRemoteDirectory(file.FullName);
                        }
                        else
                        {
                            _sftpClient.DeleteFile(file.FullName);
                        }
                    }
                }

                _sftpClient.DeleteDirectory(remotePath);
                Utils.Logger.LogSuccess($"远程目录删除成功: {remotePath}");
                return true;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"删除远程目录失败 {remotePath}: {ex.Message}");
                return false;
            }
        }
        // 释放资源
        public void Dispose()
        {
            Disconnect();
            _sftpClient?.Dispose();
            _sshClient?.Dispose();
        }
    }
}