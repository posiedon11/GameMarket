use xbox;
drop table if exists marketdetails;
create table MarketDetails(
productID char(15) not null,
productTitle varchar(200) not null,
developerName varchar (200) not null,
publisherName varchar (200) not null,
currencyCode char(5) default null,
purchasable bool not null default true,
posterImage varchar(350) default null,
msrp double default null,
listPrice double default null,
startDate datetime not null,
endDate datetime not null,

constraint free_price_null check (
(purchasable is true and msrp is not null and listprice is not null and currencyCode is not null) or
(purchasable is false and msrp is null and listprice is null and currencyCode is null)
),
foreign key (productID) references productids(productID) on delete cascade,
primary key (productID)


)