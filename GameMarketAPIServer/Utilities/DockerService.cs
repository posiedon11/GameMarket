﻿using GameMarketAPIServer.Configuration;
using System;
using System.Diagnostics;

namespace GameMarketAPIServer.Services
{
    public class DockerService
    {
        private readonly ILogger logger;
        public DockerService(ILogger _logger)
        {
            logger = _logger;
        }
        public void StartContainer(string containerName, string passWord, int hostPort)
        {
            try
            {
                // Start a container
                string command = "docker";
                string args = $"run --name {containerName} -e MYSQL_ROOT_PASSWORD={passWord} -p {hostPort}:3306 -d mysql";
                using Process dockerProcess = new Process();
                dockerProcess.StartInfo.FileName = command;
                dockerProcess.StartInfo.Arguments = args;
                dockerProcess.StartInfo.RedirectStandardOutput = true;
                dockerProcess.StartInfo.RedirectStandardError = true;
                dockerProcess.StartInfo.UseShellExecute = false;
                dockerProcess.StartInfo.CreateNoWindow = true;

                dockerProcess.Start();

                string output = dockerProcess.StandardOutput.ReadToEnd();
                string error = dockerProcess.StandardError.ReadToEnd();

                dockerProcess.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    logger.LogDebug("Error: " + error);
                }
                else
                {
                    logger.LogDebug("Output: " + output);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Exception: " + ex.Message);
            }
        }
    }
}
