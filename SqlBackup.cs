using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;

namespace RemoteToLocalSQLBackup
{
    class SqlBackup : IDisposable
    {
        SqlConnection sqlConnection = null;
        SqlCommand sqlCmd = null;
        string logTxt = string.Empty;
        string tmpDBName = "TempoBak";
        const string getDBSize = "USE [{0}] SELECT CAST(SUM(size) * 8 / 1024 AS BIGINT) FROM sys.database_files;";
        const string createBakFiles1 = "BACKUP DATABASE [{0}] TO DISK = N'{0}{1}_tmp_1.bak'";
        const string createBakFiles2 = " ,DISK = N'{0}{1}_tmp_{2}.bak'";
        const string createBakFiles3 = " WITH NOFORMAT, NOINIT, NAME = N'{0}{1}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";
        const string getBackupPath = "SELECT TOP 1 physical_device_name FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON b.media_set_id = m.media_set_id WHERE database_name = '{0}' and backup_finish_date >=N'{1:yyyy-MM-dd}' and backup_finish_date < N'{2:yyyy-MM-dd}' ORDER BY backup_finish_date DESC";
        const string tempDB = "IF db_id('{0}') IS NULL BEGIN CREATE DATABASE [{0}]; END ELSE BEGIN USE master; ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}]; CREATE DATABASE [{0}]; END";
        const string tempTbl = "USE [{0}]; CREATE TABLE Temp (filename NVARCHAR(512), [file] VARBINARY(MAX)); USE [{1}];";
        const string insertBakFile = "INSERT INTO [{0}].dbo.Temp([filename], [file]) SELECT N'{1}' AS [filename], * FROM OPENROWSET(BULK N'{1}', SINGLE_BLOB) AS [file]";
        const string bakFileName = "{0}\\{1}_tmp_{2}.bak";
        const string deleteRowsFromTemp = "DELETE FROM [{0}].dbo.Temp";
        const string deleteTempDB = "USE master; ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}];";

        string dbName = string.Empty;
        string defaultBakPath = string.Empty;
        string localPath = string.Empty;
        long dbSize = 0;
        bool useSameDB = false;
        DateTime backExt = DateTime.UtcNow;

        public SqlBackup(string connectionString, string localPath, bool useSameDBASTemp)
        {
            this.localPath = localPath;
            this.useSameDB = useSameDBASTemp;
            sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
        }

        public SqlBackup(string connectionString, string remotePath, string localPath, bool useSameDBASTemp)
        {
            this.defaultBakPath = remotePath;
            this.localPath = localPath;
            this.useSameDB = useSameDBASTemp;
            sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
        }

        public long GetDBSize()
        {
            sqlCmd = new SqlCommand(string.Format(getDBSize, sqlConnection.Database), sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            object v = sqlCmd.ExecuteScalar();
            if (v != null)
                dbSize = (long)v;
            else
                v = 1;

            sqlCmd.Dispose();

            return dbSize;
        }

        public int CreateBakupFiles()
        {
            backExt = DateTime.UtcNow;
            string script = string.Format(createBakFiles1, sqlConnection.Database, backExt.ToString("yyyyMMddHHmm"));

            int numberOfFiles = (int)(dbSize / 80); //try to make each bak file 80MB in size

            if (numberOfFiles <= 0)
                numberOfFiles = 1;

            if (numberOfFiles > 64)
                numberOfFiles = 64; //sql allows up to 64 files max

            for (int i = 2; i <= numberOfFiles; i++)
            {
                script += string.Format(createBakFiles2, sqlConnection.Database, backExt.ToString("yyyyMMddHHmm"), i);
            }

            script += string.Format(createBakFiles3, sqlConnection.Database, backExt.ToString("yyyyMMddHHmm"));

            sqlCmd = new SqlCommand(script, sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            sqlCmd.ExecuteNonQuery();

            sqlCmd.CommandText = string.Format(getBackupPath, sqlConnection.Database, DateTime.Now.Date, DateTime.Now.Date.AddDays(1));
            defaultBakPath = sqlCmd.ExecuteScalar() as string;
            defaultBakPath = Path.GetDirectoryName(defaultBakPath);

            sqlCmd.Dispose();

            return numberOfFiles;
        }

        public void CreateTempDB()
        {
            if (useSameDB)
            {
                tmpDBName = sqlConnection.Database;
            }
            string workingDBName = sqlConnection.Database;
            string cmd = string.Format(tempTbl, tmpDBName, sqlConnection.Database);
            if (!useSameDB)
            {
                sqlCmd = new SqlCommand(string.Format(tempDB, tmpDBName), sqlConnection);
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Dispose();
            }

            sqlCmd = new SqlCommand(cmd, sqlConnection);
            sqlCmd.ExecuteNonQuery();
            sqlCmd.Dispose();
        }

        public void InsertBakFile(int i)
        {
            sqlCmd = new SqlCommand(string.Format(insertBakFile, tmpDBName, string.Format(bakFileName, defaultBakPath, sqlConnection.Database + backExt.ToString("yyyyMMddHHmm"), i)), sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            sqlCmd.ExecuteNonQuery();
            sqlCmd.Dispose();
        }

        public string DownloadBakFile(int i)
        {

            string remoteFileName = string.Format(bakFileName, this.defaultBakPath, sqlConnection.Database + backExt.ToString("yyyyMMddHHmm"), i);
            string localFileName = Path.Combine(this.localPath, Path.GetFileName(remoteFileName));

            try
            {
                sqlCmd = new SqlCommand("SELECT * FROM [" + tmpDBName + "].dbo.Temp WHERE [filename] = N'" + remoteFileName + "'", sqlConnection);
                sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
                SqlDataReader sqldr = sqlCmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess);

                sqldr.Read();
                string fileName = sqldr.GetString(0);

                using (FileStream file = new FileStream(localFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    long startIndex = 0;
                    const int ChunkSize = 1024 * 32; // 32 KB block
                    byte[] buffer = new byte[ChunkSize];

                    while (true)
                    {
                        long retrievedBytes = sqldr.GetBytes(1, startIndex, buffer, 0, ChunkSize);
                        file.Write(buffer, 0, (int)retrievedBytes);
                        startIndex += retrievedBytes;
                        if (retrievedBytes != ChunkSize)
                            break;
                    }
                }

                sqldr.Close();
                sqlCmd.Dispose();

                // Delete the remote file after downloading
                DeleteRemoteBakFile(remoteFileName);
                logTxt = $"File downloaded and deleted successfully: {localFileName}";
                Console.WriteLine(logTxt);
            }
            catch (UnauthorizedAccessException ex)
            {
                logTxt = $"Access to the path '{localFileName}' is denied: {ex.Message}";
                Console.WriteLine(logTxt);
            }
            catch (FileNotFoundException ex)
            {
                logTxt = $"File not found: {remoteFileName}. Exception: {ex.Message}";
                Console.WriteLine(logTxt);
            }
            catch (Exception ex)
            {
                logTxt = $"Error downloading the file: {ex.Message}";
                Console.WriteLine(logTxt);
            }
            return logTxt;
        }
        public string CombineDownloadedBakFile(int numberOfFiles)
        {
            string logTxt = string.Empty;
            string finalFileName = Path.Combine(localPath, $"{sqlConnection.Database + backExt.ToString("yyyyMMddHHmm")}.bak");

            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(finalFileName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (FileStream finalFile = new FileStream(finalFileName, FileMode.Create, FileAccess.Write))
                {
                    for (int i = 1; i <= numberOfFiles; i++)
                    {
                        string fileName = Path.Combine(localPath, $"{sqlConnection.Database + backExt.ToString("yyyyMMddHHmm")}_tmp_{i}.bak");

                        if (File.Exists(fileName))
                        {
                            using (FileStream splitFile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                            {
                                splitFile.CopyTo(finalFile);
                            }

                            // Delete the local split file after combining
                            File.Delete(fileName);

                            Console.WriteLine($"Split file {i} added to the final backup file and deleted from local.");
                        }
                        else
                        {
                            logTxt = $"Split file {fileName} not found.";
                            Console.WriteLine(logTxt);
                        }
                    }
                }

                logTxt = $"Combined file created successfully: {finalFileName}";
                Console.WriteLine(logTxt);

                // Calculate MD5 hash for combined local file
                string localHash = CalculateMD5(finalFileName);

                // Calculate MD5 hash for remote file
                string remoteFilePath = Path.Combine(defaultBakPath, $"{sqlConnection.Database + backExt.ToString("yyyyMMddHHmm")}.bak");
                string remoteHash = CalculateMD5(remoteFilePath);

                // Compare hashes
                if (localHash == remoteHash)
                {
                    Console.WriteLine("Hashes match. Deleting remote backup files.");

                    // Delete remote backup files
                    for (int i = 1; i <= numberOfFiles; i++)
                    {
                        string remoteFileName = string.Format(bakFileName, defaultBakPath, sqlConnection.Database + backExt.ToString("yyyyMMddHHmm"), i);
                        DeleteRemoteBakFile(remoteFileName);
                    }

                    Console.WriteLine("Remote backup files deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Hashes do not match. Remote files will not be deleted.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                logTxt = $"Access to the path '{localPath}' is denied: {ex.Message}";
                Console.WriteLine(logTxt);
            }
            catch (Exception ex)
            {
                logTxt = $"Error combining the file: {ex.Message}";
                Console.WriteLine(logTxt);
            }

            return logTxt;
        }

        public void DownloadBackupFileByName(string backupFileName)
        {
            string remoteFilePath = Path.Combine(defaultBakPath, backupFileName + ".bak");
            string localFilePath = Path.Combine(localPath, backupFileName + ".bak");

            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(localFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Read the file from the remote server
                using (FileStream remoteFile = new FileStream(remoteFilePath, FileMode.Open, FileAccess.Read))
                {
                    // Write the file to the local directory
                    using (FileStream localFile = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                    {
                        remoteFile.CopyTo(localFile);
                    }
                }

                // Calculate MD5 hash for remote file
                string remoteHash = CalculateMD5(remoteFilePath);

                // Calculate MD5 hash for local file
                string localHash = CalculateMD5(localFilePath);

                // Compare hashes
                if (remoteHash == localHash)
                {
                    Console.WriteLine($"File downloaded successfully: {localFilePath}");
                    Console.WriteLine($"MD5 Hash: {localHash}");

                    // Delete remote file if hashes match
                    DeleteRemoteBackupFile(remoteFilePath);
                    Console.WriteLine($"Remote file deleted: {remoteFilePath}");
                }
                else
                {
                    Console.WriteLine($"Error: Hashes do not match. Downloaded file may be corrupted.");
                }

                Console.WriteLine($"File downloaded successfully: {localFilePath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access to the path '{localPath}' is denied: {ex.Message}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {remoteFilePath}. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading the file: {ex.Message}\n" +
                    "Remote Path : " + remoteFilePath + "\n" +
                    "Local Path : " + localFilePath);
            }
        }

        public void DeleteBakFile()
        {
            try
            {
                // Ensure the command is properly initialized
                sqlCmd = new SqlCommand(string.Format(deleteRowsFromTemp, tmpDBName), sqlConnection);
                sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
                sqlCmd.ExecuteNonQuery();
                Console.WriteLine("Temporary backup files deleted successfully from the database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting temporary backup files: {ex.Message}");
            }
            finally
            {
                if (sqlCmd != null)
                {
                    sqlCmd.Dispose();
                }
            }
        }

        public void DeleteTempDB()
        {
            try
            {
                // Delete rows from the temporary table
                sqlCmd = new SqlCommand(string.Format(deleteRowsFromTemp, tmpDBName), sqlConnection);
                sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Dispose();

                // Drop the temporary database if not using the same database
                if (!useSameDB)
                {
                    sqlCmd = new SqlCommand(string.Format(deleteTempDB, tmpDBName), sqlConnection);
                    sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
                    sqlCmd.ExecuteNonQuery();
                }

                Console.WriteLine("Temporary database and its files deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting temporary database: {ex.Message}");
            }
            finally
            {
                if (sqlCmd != null)
                {
                    sqlCmd.Dispose();
                }
            }
        }
        private void DeleteRemoteBakFile(string remoteFileName)
        {
            try
            {
                sqlCmd = new SqlCommand("DELETE FROM [" + tmpDBName + "].dbo.Temp WHERE [filename] = N'" + remoteFileName + "'", sqlConnection);
                sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Dispose();

                // Additionally, delete the file from the file system if applicable
                if (File.Exists(remoteFileName))
                {
                    File.Delete(remoteFileName);
                    logTxt = $"Remote file deleted: {remoteFileName}";
                    Console.WriteLine(logTxt);
                }
            }
            catch (Exception ex)
            {
                logTxt = $"Error deleting remote file: {ex.Message}";
                Console.WriteLine(logTxt);
            }
        }
        private void DeleteRemoteBackupFile(string remoteFilePath)
        {
            // delete direct
            try
            {
                // Add code here to delete remote file
                File.Delete(remoteFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting remote backup file: {ex.Message}");
            }
        }
        public void Dispose()
        {
            sqlCmd?.Dispose();
            sqlConnection?.Dispose();
        }

        private void CallBack(IAsyncResult ar)
        {
            SqlCommand sqlc = ((dynamic)ar.AsyncState).sqlCmd2;
            Action<object> callbak = ((dynamic)ar.AsyncState).callbak;
            SqlDataReader sqldr = sqlc.EndExecuteReader(ar);

            if (sqldr.Read())
            {
                string fileName = sqldr.GetString(0);

                using (FileStream file = new FileStream(Path.Combine(localPath, Path.GetFileName(fileName)), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    long startIndex = 0;
                    const int ChunkSize = 1024 * 8;
                    byte[] buffer = new byte[ChunkSize];
                    while (true)
                    {
                        long retrievedBytes = sqldr.GetBytes(1, startIndex, buffer, 0, ChunkSize);
                        file.Write(buffer, 0, (int)retrievedBytes);
                        startIndex += retrievedBytes;
                        if (retrievedBytes != ChunkSize)
                            break;
                    }
                }
                sqlc.Dispose();

                callbak?.Invoke(new { fname = Path.GetFileName(fileName), i = (int)((dynamic)ar.AsyncState).i });
            }
        }

        private string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
