/*
okarpov: oleksandr karpov, 2015
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace RemoteToLocalSQLBackup
{
    class SqlBackup : IDisposable
    {
        SqlConnection sqlConnection = null;
        SqlCommand sqlCmd = null;

        string tmpDBName = "TempoBak";
        const string getDBSize = "USE [{0}] SELECT CAST(SUM(size) * 8 / 1024 AS BIGINT) FROM sys.database_files;";//"USE [{0}] SELECT CAST(SUM(size) * 8. / 1024 AS BIGINT) FROM sys.master_files WITH(NOWAIT) WHERE database_id = DB_ID() GROUP BY database_id";
        const string createBakFiles1 = "BACKUP DATABASE [{0}] TO DISK = N'{0}{1}_tmp_1.bak'";
        const string createBakFiles2 = " ,DISK = N'{0}{1}_tmp_{2}.bak'";
        const string createBakFiles3 = " WITH NOFORMAT, NOINIT,  NAME = N'{0}{1}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";
        const string getBackupPath = "SELECT TOP 1 physical_device_name FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON b.media_set_id = m.media_set_id WHERE database_name = '{0}' and backup_finish_date >=N'{1:yyyy-MM-dd}' and backup_finish_date < N'{2:yyyy-MM-dd}' ORDER BY backup_finish_date DESC";
        //const string tempDB = "IF db_id('TempoBak') IS NULL begin create database TempoBak; end else begin use master; ALTER DATABASE TempoBak SET SINGLE_USER WITH ROLLBACK IMMEDIATE; drop database TempoBak; create database TempoBak; end use TempoBak; create table Temp (filename nvarchar(512), [file] varbinary(max)); use {0};";
        const string tempDB = "IF db_id('{0}') IS NULL begin create database [{0}]; end else begin use master; ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; drop database [{0}]; create database [{0}]; end";
        const string tempTbl = "use [{0}]; create table Temp (filename nvarchar(512), [file] varbinary(max)); use [{1}];";
        const string insertBakFile = "INSERT INTO [{0}].dbo.Temp([filename], [file]) SELECT N'{1}' as [filename], * FROM OPENROWSET(BULK N'{1}', SINGLE_BLOB) AS [file]";
        const string bakFileName = "{0}\\{1}_tmp_{2}.bak";
        const string deleteRowsFromTemp = "DELETE FROM [{0}].dbo.Temp";
        const string deleteTempDB = "use master; ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; drop database [{0}];";

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

        /// <summary>
        /// Get size of DB in MB
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create BAK files (default backup path)
        /// </summary>
        /// <returns></returns>
        public int CreateBakupFiles()
        {
            backExt = DateTime.UtcNow;
            string script = string.Format(createBakFiles1, sqlConnection.Database, backExt.ToString("yyyyMMddhhmm"));

            int numberOfFiles = (int)(dbSize / 80); //try to make each bak file 80MB in size

            if (numberOfFiles <= 0)
                numberOfFiles = 1;

            if (numberOfFiles > 64)
                numberOfFiles = 64; //sql allows up to 64 files max

            for (int i = 2; i <= numberOfFiles; i++)
            {
                script += string.Format(createBakFiles2, sqlConnection.Database, backExt.ToString("yyyyMMddhhmm"), i);
            }

            script += string.Format(createBakFiles3, sqlConnection.Database, backExt.ToString("yyyyMMddhhmm"));

            sqlCmd = new SqlCommand(script, sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            sqlCmd.ExecuteNonQuery();

            sqlCmd.CommandText = string.Format(getBackupPath, sqlConnection.Database, DateTime.Now.Date, DateTime.Now.Date.AddDays(1));
            defaultBakPath = sqlCmd.ExecuteScalar() as string;
            defaultBakPath = System.IO.Path.GetDirectoryName(defaultBakPath);

            sqlCmd.Dispose();

            return numberOfFiles;
        }

        /// <summary>
        /// Create temp DB and table
        /// </summary>
        public void CreateTempDB()
        {
            if(useSameDB)
            {
                tmpDBName = sqlConnection.Database;
                //return;
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

        /// <summary>
        /// Insert a bak file into temp table
        /// </summary>
        /// <param name="i"></param>
        public void InsertBakFile(int i)
        {
            sqlCmd = new SqlCommand(string.Format(insertBakFile, tmpDBName, string.Format(bakFileName, this.defaultBakPath, sqlConnection.Database+ backExt.ToString("yyyyMMddhhmm"), i)), sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            sqlCmd.ExecuteNonQuery();

            sqlCmd.Dispose();
        }

        /// <summary>
        /// Starts downloading asynchronously
        /// </summary>
        /// <param name="i"></param>
        /// <param name="callbak"></param>
        public void DownloadBakFile(int i, Action<object> callbak)
        {
            SqlCommand sqlCmd2 = new SqlCommand("SELECT * FROM [" + tmpDBName + "].dbo.Temp WHERE [filename] = " +
                string.Format(bakFileName, this.defaultBakPath, sqlConnection.Database, i), new SqlConnection(sqlConnection.ConnectionString));
            sqlCmd2.CommandTimeout = sqlConnection.ConnectionTimeout;
            sqlCmd2.Connection.Open();
            sqlCmd2.BeginExecuteReader(new AsyncCallback(CallBack), new { sqlCmd2, callbak, i }, System.Data.CommandBehavior.SequentialAccess);
        }

        /// <summary>
        /// Download bak file
        /// </summary>
        /// <param name="i"></param>
        /// <param name="callbak"></param>
        public void DownloadBakFile(int i)
        {
            sqlCmd = new SqlCommand("SELECT * FROM [" + tmpDBName + "].dbo.Temp WHERE [filename] = N'" +
                string.Format(bakFileName, this.defaultBakPath, sqlConnection.Database, i) + "'", sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            SqlDataReader sqldr = sqlCmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess);

            sqldr.Read();
            string fileName = sqldr.GetString(0);


            System.IO.FileStream file = new System.IO.FileStream(System.IO.Path.Combine(this.localPath, System.IO.Path.GetFileName(fileName)),
                System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);

            long startIndex = 0;

            const int ChunkSize = 1024 * 32; //32 KB block

            byte[] buffer = new byte[ChunkSize];
            while (true)
            {
                long retrievedBytes = sqldr.GetBytes(1, startIndex, buffer, 0, ChunkSize);
                file.Write(buffer, 0, (int)retrievedBytes);
                startIndex += retrievedBytes;
                if (retrievedBytes != ChunkSize)
                    break;
            }

            file.Close();
            sqlCmd.Dispose();
        }

        /// <summary>
        /// delete bak file
        /// </summary>
        /// <param name="i"></param>
        public void DeleteBakFile()
        {
            sqlCmd = new SqlCommand(string.Format(deleteRowsFromTemp, tmpDBName), sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            sqlCmd.ExecuteNonQuery();
            sqlCmd.Dispose();
        }

        /// <summary>
        /// Delete temp rows and temp db
        /// </summary>
        public void DeleteTempDB()
        {
            sqlCmd = new SqlCommand(string.Format(deleteRowsFromTemp, tmpDBName), sqlConnection);
            sqlCmd.CommandTimeout = sqlConnection.ConnectionTimeout;
            sqlCmd.ExecuteNonQuery();
            if (!useSameDB)
            {
                sqlCmd.CommandText = string.Format(deleteTempDB, tmpDBName);
                sqlCmd.ExecuteNonQuery();
            }
            sqlCmd.Dispose();
        }

        void CallBack(IAsyncResult ar)
        {
            SqlCommand sqlc = ((dynamic)ar.AsyncState).sqlCmd2;
            Action<object> callbak = ((dynamic)ar.AsyncState).callbak;
            SqlDataReader sqldr = sqlc.EndExecuteReader(ar);
            sqldr.Read();
            string fileName = sqldr.GetString(0);


            System.IO.FileStream file = new System.IO.FileStream(System.IO.Path.Combine(this.localPath, System.IO.Path.GetFileName(fileName)), 
                System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);

            long startIndex = 0;

            const int ChunkSize = 1024 * 8;

            while (true)
            {
                byte[] buffer = new byte[ChunkSize];
                long retrievedBytes = sqldr.GetBytes(1, startIndex, buffer, 0, ChunkSize);
                file.Write(buffer, 0, (int)retrievedBytes);
                startIndex += retrievedBytes;
                if (retrievedBytes != ChunkSize)
                    break;
            }

            file.Close();
            sqlc.Dispose();

            if (callbak != null)
                callbak.Invoke(new { fname = System.IO.Path.GetFileName(fileName), i = (int)((dynamic)ar.AsyncState).i });

        }

        public void Dispose()
        {
            if (sqlConnection != null)
            {
                sqlConnection.Dispose();
            }
            sqlConnection = null;
        }
    }
}
