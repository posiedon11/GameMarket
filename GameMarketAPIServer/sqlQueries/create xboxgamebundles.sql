use xbox;

drop table if exists gamebundles;
create table gamebundles(
relatedProductID char(15) not null,
productID char(15) not null,
foreign key (productID) references titledetails(productID) on delete cascade,
foreign key (relatedProductID) references productids(productID) on delete cascade,
primary key (relatedProductID)
);
