use xbox;

drop table if exists productPlatforms;

create table productPlatforms(
productID char(15) not null,
platform varchar(40) not null,
foreign key (productID) references productids(productID) on delete cascade,
primary key(productID, platform)
);