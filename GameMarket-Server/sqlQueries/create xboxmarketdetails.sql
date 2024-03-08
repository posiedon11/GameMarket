use gamemarket;

create table xboxMarketDetails(
productID char(15) not null,
developerName varchar (40) not null,
publisherName varchar (40) not null,
currencyCode char(5) not null default "USA",
purchasable bool default true,
posterImage varchar(300) not null,
msrp float not null default 0.0,
listPrice float not null default 0.0,
startDate datetime not null,
endDate datetime not null,
primary key (productID)
)