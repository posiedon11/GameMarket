use xbox;

drop table if exists productids;
create table ProductIds(
productID char(15) not null,
lastScanned datetime default null,
primary key (productID)
)