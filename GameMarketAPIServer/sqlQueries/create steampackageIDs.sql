use steam;
drop table if exists packageIds;
create table packageIDs(
packageID int unsigned not null,
lastScanned datetime default null,
primary key(packageID)
);