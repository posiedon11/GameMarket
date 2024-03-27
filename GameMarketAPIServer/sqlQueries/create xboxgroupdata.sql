use xbox;
drop table if exists groupdata;

create table groupData(
groupID varchar(50) not null,
groupName varchar(150) not null,
primary key(groupID)
);