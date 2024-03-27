use steam;
drop table if exists appdetails;
create table appdetails(
appID int unsigned not null,
appType varchar(30) not null,
appName varchar(200) not null,
msrp double default null,
listprice double default null,
isFree bool not null default false,
lastScanned datetime default now(),

constraint free_price_null check (
(isFree is false and msrp is not null and listprice is not null) or
(isFree is true and msrp is null and listprice is null)
),
foreign key (appID) references appIDs(appID) on delete cascade,
primary key(appID)
);