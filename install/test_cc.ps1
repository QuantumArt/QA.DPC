$i = @{
    databaseServer = 'mscpgsql01';
    targetBackupPath = 'C:\users\celutP\sber.dump';
    customerCode = 'test_catalog';
    currentSqlPath = '\\storage\Developers_share\QP.PG\current.sql'; 
    siteSyncHost = 'http://localhost:8013'; 
    syncApiHost = 'http://localhost:8015'; 
    elasticsearchHost = 'http://node1:9200; http://node2:9200'; 
    adminHost = 'http://localhost:89/Dpc.Admin';
    login = 'postgres';
    password = '1q2w-p=[';
    customerLogin = 'login';
    customerPassword = '1q2w#E$R'; 
    dbType = 1;
}

$d = @{
    server = 'mscpgsql01';
    database = 'test_catalog';
    name = 'postgres';
    pass = '1q2w-p=[';
    dbType = 1 ;
}