use steam;
drop table if exists PackageDetails;
create table PackageDetails(
packageID int unsigned not null,
packageName varchar(50) not null,
listprice float not null,
msrp float not null,
foreign key (packageID) references packageIDs(packageID) on delete cascade,
primary key (packageID)
);