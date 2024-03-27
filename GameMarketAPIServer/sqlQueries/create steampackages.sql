use steam;
drop table if exists packages;
create table packages(
appID int unsigned not null,
packageID int unsigned not null,
foreign key (appID) references	appIDs(appID)on delete cascade,
foreign key(packageID) references packageIDs(packageID)on delete cascade,
primary key(appID, packageID)
);

