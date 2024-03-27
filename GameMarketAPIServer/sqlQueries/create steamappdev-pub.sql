use steam;

drop table if exists appDevelopers;
drop table if exists appPublishers;
create table appDevelopers(
appID int unsigned not null,
developer varchar (30) not null,
foreign key (appID) references appIDs(appID) on delete cascade,
primary key (appID, developer)
);
create table appPublishers(
appID int unsigned not null,
publisher varchar (30) not null,
foreign key (appID) references appIDs(appID) on delete cascade,
primary key (appID, publisher)
);