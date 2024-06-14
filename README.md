# RemoteToLocalSQLBackup
Each of us had issue with remote SQL backup files. Not with files itself but actually how to get bak files from remote sql server to local PC
This solution in general looks like this:
- execute an sql script to make backup of database;
- create a temp db with table and column of type varbinary type;
- get the *.bak file and insert it into the temp table;
- stream this row to your local pc and save as file;
- drop temp table and db;

More info at my blog post there: https://www.ok.unsode.com/post/2015/06/27/remote-sql-backup-to-local-pc
Forked from : https://github.com/okarpov/RemoteToLocalSQLBackup