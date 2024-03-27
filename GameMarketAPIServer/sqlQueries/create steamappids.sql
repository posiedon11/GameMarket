use steam;
drop table if exists appIDs;
create table appIDs(
appID int unsigned not null,
lastScanned datetime default null,
primary key (appID)
);