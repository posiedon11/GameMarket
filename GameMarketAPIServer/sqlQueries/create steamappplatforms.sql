use steam;

drop table if exists appPlatforms;
create table appPlatforms(
appID int unsigned not null,
platform varchar(10) not null,
foreign key (appID) references appIds(appID) on delete cascade,
primary key(appID)
)